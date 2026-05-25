using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;

namespace DuelApp.Modules.Matchmaking.Application.Abstractions;

public interface IMatchmakingRepository
{
    Task AddAsync(QueueEntry entry);
    Task<QueueEntry?> GetByPlayerIdAsync(Guid playerId);
    Task<bool> IsUserInQueueAsync(Guid playerId);
    Task<List<QueueEntry>> GetQueuedBatchAsync(int batchSize);
    Task UpdateAsync(QueueEntry entry);
    Task BulkUpdateAsync(IEnumerable<QueueEntry> entries);
}