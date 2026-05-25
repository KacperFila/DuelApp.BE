using DuelApp.Modules.Duels.Domain.Duels.Enums;
using DuelApp.Modules.Duels.Domain.Duels.ValueObjects;
using DuelApp.Shared.Abstractions.Kernel.Types;

namespace DuelApp.Modules.Duels.Domain.Duels.Entities;

public sealed class Duel : AggregateRoot<Guid>
{
    public Guid PlayerOneId { get; private set; } = Guid.Empty;
    public Guid PlayerTwoId { get; private set; } = Guid.Empty;
    public DuelStatus Status { get; private set; } = DuelStatus.None;
    public int CurrentRound { get; private set; } = 0;
    public int TotalRounds { get; private set; } = 0;
    public List<DuelRound> Rounds { get; private set; } = [];
    public int PlayerOneScore { get; private set; } = 0;
    public int PlayerTwoScore { get; private set; } = 0;
    public Guid WinnerId { get; private set; } = Guid.Empty;
    public DateTime StartedAt { get; private set; } = DateTime.MinValue;
    public DateTime FinishedAt { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Creates a new duel between two players with the specified number of rounds.
    /// Initializes the duel with the first round and sets its status to InProgress.
    /// </summary>
    /// <param name="player1Id">The unique identifier of the first player.</param>
    /// <param name="player2Id">The unique identifier of the second player.</param>
    /// <param name="totalRounds">The total number of rounds in the duel. Must be greater than zero.</param>
    /// <exception cref="InvalidOperationException">Thrown when both player IDs are the same.</exception>
    /// <exception cref="ArgumentException">Thrown when totalRounds is less than or equal to zero.</exception>
    public static Duel Create(Guid player1Id, Guid player2Id, int totalRounds)
    {
        if (player1Id == player2Id)
        {
            throw new InvalidOperationException("Players must be different.");
        }

        if (totalRounds <= 0)
        {
            throw new ArgumentException("Total rounds must be greater than zero.");
        }
        
        return new Duel
        {
            Id = Guid.NewGuid(),
            PlayerOneId = player1Id,
            PlayerTwoId = player2Id,
            TotalRounds = totalRounds,
            CurrentRound = 0,
            Rounds = [],
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

        if (!round.IsCompleted())
        {
            return;
        }
        
        ApplyRoundResult(round, isCorrect);

        if (IsLastRound())
        {
            Finish();
        }
    }
    
    private void ApplyRoundResult(DuelRound round, bool isCorrect)
    {
        if (!isCorrect)
        {
            return;
        }

        if (round.HasPlayerOneAnsweredCorrectly == true)
        {
            PlayerOneScore++;
        }
        
        if (round.HasPlayerTwoAnsweredCorrectly == true)
        {
            PlayerTwoScore++;
        }
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
    
    private void Finish()
    {
        Status = DuelStatus.Completed;
        FinishedAt = DateTime.UtcNow;

        WinnerId = PlayerOneScore > PlayerTwoScore
            ? PlayerOneId
            : PlayerTwoScore > PlayerOneScore
                ? PlayerTwoId
                : Guid.Empty;
    }
    
    private void EnsureInProgress()
    {
        if (Status != DuelStatus.InProgress)
        {
            throw new InvalidOperationException("Duel is not in progress.");
        }
    }

    private DuelRound GetCurrentRound()
    {
        return Rounds.Single(x => x.Number == CurrentRound);
    }

    public void CreateNextRound(Guid questionId)
    {
        Rounds.Add(DuelRound.Create(
            CurrentRound + 1,
            questionId));
    }
}