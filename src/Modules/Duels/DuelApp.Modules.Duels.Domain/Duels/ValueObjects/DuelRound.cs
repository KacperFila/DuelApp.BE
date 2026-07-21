using DuelApp.Modules.Duels.Domain.Duels.Enums;

namespace DuelApp.Modules.Duels.Domain.Duels.ValueObjects;

public sealed class DuelRound
{
    public int Number { get; }
    public Guid QuestionId { get; set; }
    public bool HasPlayerOneSubmittedAnswer { get; set; }
    public bool HasPlayerTwoSubmittedAnswer { get; set; }
    public bool HasPlayerOneAnsweredCorrectly { get; set; }
    public bool HasPlayerTwoAnsweredCorrectly { get; set; }
    public DuelRoundStatus Status { get; set; }

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
        
        Number = number;
        QuestionId = questionId;
        Status = DuelRoundStatus.InProgress;
    }

    public static DuelRound Create(int number, Guid questionId)
    {
        return new DuelRound(number, questionId);
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