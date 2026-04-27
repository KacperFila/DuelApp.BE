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
            CurrentRound = 1,
            Rounds = [DuelRound.Create(1)],
            StartedAt = DateTime.UtcNow,
            Status = DuelStatus.InProgress
        };
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
        AdvanceRoundOrFinish();
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
    
    private void AdvanceRoundOrFinish()
    {
        if (CurrentRound >= TotalRounds)
        {
            Finish();
            return;
        }

        CurrentRound++;
        Rounds.Add(DuelRound.Create(CurrentRound));
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
}