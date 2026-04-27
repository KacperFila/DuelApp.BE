using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;

namespace DuelApp.Modules.Matchmaking.Application.Abstractions;

public interface IMatchmakingRepository
{
    Task AddAsync(MatchmakingQueueEntry entry, CancellationToken ct = default);
    Task<MatchmakingQueueEntry?> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default);
    Task<List<MatchmakingQueueEntry>> GetQueuedAsync(CancellationToken ct = default);
    Task UpdateAsync(MatchmakingQueueEntry entry, CancellationToken ct = default);
}