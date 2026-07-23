using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using DuelApp.Modules.Duels.Domain.Duels.Enums;
using Microsoft.EntityFrameworkCore;

namespace DuelApp.Modules.Duels.Infrastructure.EF.Repositories;

public class DuelsRepository : IDuelsRepository
{
    private readonly DuelsDbContext _dbContext;

    public DuelsRepository(DuelsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Duel?> GetByIdAsync(Guid duelId)
    {
        return await _dbContext.Duels.FirstOrDefaultAsync(x => x.Id == duelId);
    }

    public async Task<Duel?> GetForUpdateByRoundIdAsync(Guid roundId)
    {
        var duelId = await _dbContext.Duels
            .Where(x => x.Rounds.Any(round => round.Id == roundId))
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync();

        return duelId is null
            ? null
            : await GetForUpdateById(duelId.Value);
    }

    public async Task<bool> IsPlayerCurrentlyInDuelAsync(Guid playerId)
    {
        return await _dbContext.Duels
            .AnyAsync(x => x.Id == playerId 
                    && x.Status == DuelStatus.InProgress);
    }

    public async Task CreateDuelAsync(Duel duel)
    {
        _dbContext.Duels.Add(duel);
        await _dbContext.SaveChangesAsync();
    }

    public Task UpdateDuelAsync(Duel duel)
    {
        _dbContext.Duels.Update(duel);
        return Task.CompletedTask;
    }
    
    public async Task<Guid?> GetCurrentDuelIdForPlayerAsync(Guid playerId)
    {
        return await _dbContext.Duels
            .Where(x =>
                x.Status == DuelStatus.Pending || x.Status == DuelStatus.InProgress &&
                (x.PlayerOneId == playerId ||
                 x.PlayerTwoId == playerId))
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
    }
    
    public async Task<Duel?> GetForUpdateById(Guid duelId)
    {
        if (duelId == Guid.Empty)
        {
            return null;
        }
        
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            SELECT 1
            FROM ""Duels"".""Duels""
            WHERE ""Id"" = {duelId}
            FOR UPDATE
        ");

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            SELECT 1
            FROM ""Duels"".""DuelRounds""
            WHERE ""DuelId"" = {duelId}
            FOR UPDATE
        ");
        
        return await _dbContext.Duels
            .Include(x => x.Rounds)
            .SingleAsync(x => x.Id == duelId);
    }
}
