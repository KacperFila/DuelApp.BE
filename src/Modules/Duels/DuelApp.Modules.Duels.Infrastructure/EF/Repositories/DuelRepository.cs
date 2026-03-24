using DuelApp.Modules.Duels.Domain.Duels.Entities;
using DuelApp.Modules.Duels.Domain.Duels.Repositories;

namespace DuelApp.Modules.Duels.Infrastructure.EF.Repositories;

public class DuelRepository : IDuelRepository
{
    private readonly DuelsDbContext dbContext;

    public DuelRepository(DuelsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Duel?> GetAsync(Guid duelId)
    {
        return await dbContext.Duels.FindAsync(duelId);
    }

    public async Task AddAsync(Duel? duel)
    {
        dbContext.Duels.Add(duel);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Duel? duel)
    {
        dbContext.Duels.Update(duel);
        await dbContext.SaveChangesAsync();
    }
}