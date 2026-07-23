using DuelApp.Modules.Duels.Domain.Duels.Enums;

namespace DuelApp.Modules.Duels.Domain.Duels.ValueObjects;

public sealed class DuelRound
{
    public Guid Id { get; set; }
    public int Number { get; }
    public Guid QuestionId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public bool HasPlayerOneSubmittedAnswer { get; set; }
    public bool HasPlayerTwoSubmittedAnswer { get; set; }
    public bool HasPlayerOneAnsweredCorrectly { get; set; }
    public bool HasPlayerTwoAnsweredCorrectly { get; set; }
    public DuelRoundStatus Status { get; set; }

    public DuelRound()
    {
    }
    
    private DuelRound(int number, Guid questionId)
    {
        if (number <= 0)
        {
            throw new ArgumentException("Round number must be greater than zero.");
        }

        if (questionId == Guid.Empty)
        {
            throw new ArgumentException("Question ID must be set.");
        }
        
        Id = Guid.NewGuid();
        Number = number;
        QuestionId = questionId;
        Status = DuelRoundStatus.Pending;
    }

    public static DuelRound Create(int number, Guid questionId)
    {
        return new DuelRound(number, questionId);
    }

    public void Start(TimeSpan duration)
    {
        StartedAt = DateTime.UtcNow;
        EndsAt = StartedAt.Value.Add(duration);
        Status = DuelRoundStatus.InProgress;
    }
    
    public void Expire()
    {
        if (DateTime.UtcNow < EndsAt)
        {
            throw new InvalidOperationException(
                "Round has not expired yet.");
        }
        
        if (Status !=  DuelRoundStatus.InProgress)
        {
            throw new InvalidOperationException("Only active rounds can expire.");
        }

        Status = DuelRoundStatus.Expired;
        FinishedAt = DateTime.UtcNow;
    }
    
    public bool IsCompleted()
    {
        return Status == DuelRoundStatus.Completed;
    }
    
    public void SubmitAnswer(DuelPlayer player, bool isCorrect)
    {
        if (IsCompleted())
        {
            throw new InvalidOperationException("Round already completed.");
        }
        
        var hasPlayerAnswered = HasPlayerAnswered(player);
        if (hasPlayerAnswered)
        {
            throw new InvalidOperationException("Player has already answered.");
        }

        switch (player)
        {
            case DuelPlayer.Player1:
                HasPlayerOneAnsweredCorrectly = isCorrect;
                HasPlayerOneSubmittedAnswer = true;
                break;
            case DuelPlayer.Player2:
                HasPlayerTwoAnsweredCorrectly = isCorrect;
                HasPlayerTwoSubmittedAnswer = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(player), player, null);
        }

        if (HasPlayerOneSubmittedAnswer && HasPlayerTwoSubmittedAnswer)
        {
            Status = DuelRoundStatus.Completed;
        }
    }

    private bool HasPlayerAnswered(DuelPlayer player)
    {
        return player switch
        {
            DuelPlayer.Player1 => HasPlayerOneSubmittedAnswer is true,
            DuelPlayer.Player2 => HasPlayerTwoSubmittedAnswer is true,
            _ => throw new ArgumentOutOfRangeException(nameof(player), player, null)
        };
    }
}