using DuelApp.Modules.Duels.Domain.Duels.Enums;
using DuelApp.Modules.Duels.Domain.Duels.Events;
using DuelApp.Modules.Duels.Domain.Duels.ValueObjects;
using DuelApp.Shared.Abstractions.Kernel.Types;

namespace DuelApp.Modules.Duels.Domain.Duels.Entities;

public sealed class Duel : AggregateRoot<Guid>
{
    public Guid PlayerOneId { get; private set; } = Guid.Empty;
    public Guid PlayerTwoId { get; private set; } = Guid.Empty;
    public DuelStatus Status { get; private set; } = DuelStatus.None;
    public int CurrentRound { get; private set; }
    public int TotalRounds { get; private set; }
    public List<DuelRound> Rounds { get; private set; } = [];
    public int PlayerOneScore { get; private set; }
    public int PlayerTwoScore { get; private set; }
    public Guid WinnerId { get; private set; } = Guid.Empty;
    public DateTime StartedAt { get; private set; } = DateTime.MinValue;
    public DateTime FinishedAt { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Creates a new duel between two different players using the provided rounds.
    /// Initializes the duel with a status of <see cref="DuelStatus.InProgress"/>.
    /// </summary>
    /// <param name="player1Id">The unique identifier of the first player.</param>
    /// <param name="player2Id">The unique identifier of the second player.</param>
    /// <param name="rounds">The collection of rounds that will be part of the duel. Must contain at least one round.</param>
    /// <returns>A newly created <see cref="Duel"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when both player IDs are the same.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the rounds collection is empty.
    /// </exception>
    public static Duel Create(Guid player1Id, Guid player2Id, List<DuelRound> rounds)
    {
        if (player1Id == player2Id)
        {
            throw new InvalidOperationException("Players must be different.");
        }

        if (rounds.Count <= 0)
        {
            throw new ArgumentException("Total rounds must be greater than zero.");
        }
        
        return new Duel
        {
            Id = Guid.NewGuid(),
            PlayerOneId = player1Id,
            PlayerTwoId = player2Id,
            TotalRounds = rounds.Count,
            CurrentRound = 1,
            Rounds = rounds.ToList(),
            StartedAt = DateTime.UtcNow,
            Status = DuelStatus.InProgress
        };
    }

    /// <summary>
    /// Marks the duel as completed due to a player abandoning it.
    /// </summary>
    /// <param name="abandoningPlayerId">
    /// The ID of the player who abandoned the duel. The opponent will be set as the winner.
    /// </param>
    public void Abandon(Guid abandoningPlayerId)
    {
        Status = DuelStatus.Completed;
        FinishedAt = DateTime.UtcNow;

        WinnerId = abandoningPlayerId == PlayerOneId
            ? PlayerTwoId
            : PlayerOneId;
    }
    
    public void SubmitAnswer(Guid playerId, bool isCorrect)
    {
        EnsureInProgress();

        var player = ResolvePlayer(playerId);
        var round = GetCurrentRound();

        round.SubmitAnswer(player, isCorrect);

        if (round.IsCompleted())
        {
            if (IsLastRound())
            {
                Complete();
                AddEvent(new RoundCompletedEvent(Id, round.Number, PlayerOneId, PlayerTwoId, IsDuelCompleted: true));
                return;
            }

            MoveToNextRound();
            var nextRound = GetCurrentRound();
            AddEvent(new RoundCompletedEvent(
                Id,
                round.Number,
                PlayerOneId,
                PlayerTwoId,
                IsDuelCompleted: false,
                NextRoundNumber: nextRound.Number,
                TotalRounds: TotalRounds,
                NextQuestionId: nextRound.QuestionId));
        }
    }
    
    public DuelRound GetCurrentRound()
    {
        return Rounds.Single(x => x.Number == CurrentRound);
    }
    
    private void EnsureInProgress()
    {
        if (Status != DuelStatus.InProgress)
        {
            throw new InvalidOperationException("Duel is not in progress.");
        }
    }
    
    private void MoveToNextRound()
    {
        CurrentRound++;
    }
    
    private void Complete()
    {
        Status = DuelStatus.Completed;
        FinishedAt = DateTime.UtcNow;

        PlayerOneScore = Rounds.Count(x => x.HasPlayerOneAnsweredCorrectly);
        PlayerTwoScore = Rounds.Count(x => x.HasPlayerTwoAnsweredCorrectly);
        
        WinnerId = PlayerOneScore > PlayerTwoScore
            ? PlayerOneId
            : PlayerTwoScore > PlayerOneScore
                ? PlayerTwoId
                : Guid.Empty;
    }
    
    private DuelPlayer ResolvePlayer(Guid playerId)
    {
        if (playerId == PlayerOneId)
        {
            return DuelPlayer.Player1;
        }

        if (playerId == PlayerTwoId)
        {
            return DuelPlayer.Player2;
        }
        
        throw new InvalidOperationException("Player does not belong to this duel.");
    }
    
    private bool IsLastRound()
    {
        return CurrentRound >= TotalRounds;
    }
}
