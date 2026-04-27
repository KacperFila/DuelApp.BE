using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Enums;
using DuelApp.Shared.Abstractions.Kernel.Types;

namespace DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;

public sealed class MatchmakingQueueEntry : AggregateRoot<Guid>
{
    public Guid PlayerId { get; private set; } = Guid.Empty;
    public MatchmakingStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; } = DateTime.MinValue;
    public DateTime? ExpiresAt { get; private set; }
    
    public static MatchmakingQueueEntry Create(Guid playerId)
    {
        return new MatchmakingQueueEntry
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            StartedAt = DateTime.UtcNow,
            Status = MatchmakingStatus.Queued,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2)
        };
    }
    
    public void MarkAsInMatch()
    {
        if (Status != MatchmakingStatus.Queued)
        {
            return;
        }

        Status = MatchmakingStatus.InMatch;
    }

    public void MarkAsMatched()
    {
        if (Status != MatchmakingStatus.InMatch)
        {
            return;
        }

        Status = MatchmakingStatus.Matched;
    }
}