using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
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

    public async Task<bool> IsPlayerCurrentlyInDuelAsync(Guid playerId)
    {
        return await _dbContext.Duels.AnyAsync(x => x.Id == playerId);
    }

    public async Task CreateDuelAsync(Duel? duel)
    {
        _dbContext.Duels.Add(duel);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateDuelAsync(Duel duel)
    {
        _dbContext.Duels.Update(duel);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Duel?> GetCurrentDuelForPlayerAsync(Guid playerId)
    {
        return await _dbContext.Duels.FirstOrDefaultAsync(
            x => x.PlayerOneId == playerId
                    || x.PlayerTwoId == playerId
            );
    }
}