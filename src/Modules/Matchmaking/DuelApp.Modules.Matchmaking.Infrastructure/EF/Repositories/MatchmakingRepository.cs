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

    public async Task AddAsync(MatchmakingQueueEntry entry, CancellationToken ct = default)
    {
        await _dbContext.MatchmakingQueueEntries.AddAsync(entry, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<MatchmakingQueueEntry?> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default)
    {
        return await _dbContext.MatchmakingQueueEntries
            .FirstOrDefaultAsync(x => x.PlayerId == playerId, ct);
    }

    public async Task<List<MatchmakingQueueEntry>> GetQueuedAsync(CancellationToken ct = default)
    {
        return await _dbContext.MatchmakingQueueEntries
            .Where(x => x.Status == MatchmakingStatus.Queued)
            .OrderBy(x => x.StartedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(MatchmakingQueueEntry entry, CancellationToken ct = default)
    {
        _dbContext.MatchmakingQueueEntries.Update(entry);
        await _dbContext.SaveChangesAsync(ct);
    }
}