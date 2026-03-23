using DuelApp.Modules.Duels.Domain.Duels.Entities.Enums;

namespace DuelApp.Modules.Duels.Domain.Duels.ValueObjects;

public sealed class DuelRound
{
    public int Number { get; }
    public Guid QuestionId { get; set; } = Guid.Empty;
    public bool? HasPlayerOneAnsweredCorrectly { get; set; }
    public bool? HasPlayerTwoAnsweredCorrectly { get; set; }

    private DuelRound(int number)
    {
        if (number <= 0)
        {
            throw new ArgumentException("Round number must be greater than zero.");
        }

        Number = number;
    }

    public static DuelRound Create(int number)
    {
        return new DuelRound(number);
    }

    public bool IsCompleted()
    {
        return HasPlayerOneAnsweredCorrectly.HasValue && HasPlayerTwoAnsweredCorrectly.HasValue;
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
            throw new InvalidOperationException("Player has already been answered.");
        }

        switch (player)
        {
            case DuelPlayer.Player1:
                HasPlayerOneAnsweredCorrectly = isCorrect;
                break;
            case DuelPlayer.Player2:
                HasPlayerTwoAnsweredCorrectly = isCorrect;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(player), player, null);
        }
    }

    private bool HasPlayerAnswered(DuelPlayer player)
    {
        return player switch
        {
            DuelPlayer.Player1 => HasPlayerOneAnsweredCorrectly.HasValue,
            DuelPlayer.Player2 => HasPlayerTwoAnsweredCorrectly.HasValue,
            _ => throw new ArgumentOutOfRangeException(nameof(player), player, null)
        };
    }
}