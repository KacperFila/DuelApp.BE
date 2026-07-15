using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Enums;
using DuelApp.Shared.Abstractions.Kernel.Types;

namespace DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;

public sealed class QueueEntry : AggregateRoot<Guid>
{
    public Guid PlayerId { get; private set; } = Guid.Empty;
    public MatchmakingStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; } = DateTime.MinValue;
    public DateTime? ExpiresAt { get; private set; }
    
    public static QueueEntry Create(Guid playerId)
    {
        return new QueueEntry
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            StartedAt = DateTime.UtcNow,
            Status = MatchmakingStatus.Queued,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2)
        };
    }

    public void MarkAsCancelled()
    {
        if (Status != MatchmakingStatus.Queued)
        {
            return;
        }

        Status = MatchmakingStatus.Cancelled;
    }
    
    public void MarkAsMatched()
    {
        if (Status != MatchmakingStatus.Queued)
        {
            return;
        }

        Status = MatchmakingStatus.Matched;
    }
}