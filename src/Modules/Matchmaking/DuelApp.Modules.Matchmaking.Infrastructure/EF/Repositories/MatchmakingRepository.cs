using DuelApp.Modules.Matchmaking.Application.Abstractions;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Enums;
using Microsoft.EntityFrameworkCore;

namespace DuelApp.Modules.Matchmaking.Infrastructure.EF.Repositories;

public sealed class MatchmakingRepository : IMatchmakingRepository
{
    private readonly MatchmakingDbContext _dbContext;

    public MatchmakingRepository(MatchmakingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(QueueEntry entry)
    {
        await _dbContext.QueueEntries.AddAsync(entry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<QueueEntry?> GetByPlayerIdAsync(Guid playerId)
    {
        return await _dbContext.QueueEntries
            .FirstOrDefaultAsync(x => x.PlayerId == playerId);
    }

    public async Task<bool> IsUserInQueueAsync(Guid playerId)
    {
        return await _dbContext.QueueEntries.AnyAsync(
            x => x.PlayerId == playerId 
                 && x.Status == MatchmakingStatus.Queued);
    }

    public async Task<List<QueueEntry>> GetQueuedBatchAsync(int batchSize)
    {
        return await _dbContext.QueueEntries
            .FromSqlRaw("""
                        
                                    SELECT *
                                    FROM "Matchmaking".queue_entries
                                    WHERE "Status" = {0}
                                    ORDER BY "StartedAt"
                                    FOR UPDATE SKIP LOCKED
                                    LIMIT {1}
                                    
                        """,
            MatchmakingStatus.Queued.ToString(),
            batchSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(QueueEntry entry)
    {
        _dbContext.QueueEntries.Update(entry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task BulkUpdateAsync(IEnumerable<QueueEntry> entries)
    {
        _dbContext.UpdateRange(entries);
        await _dbContext.SaveChangesAsync();
    }
}