using DuelApp.Modules.Duels.Domain.Duels.Entities;

namespace DuelApp.Modules.Duels.Domain.Duels.Repositories;

public interface IDuelRepository
{
    Task<Duel?> GetAsync(Guid duelId);
    Task AddAsync(Duel? duel);
    Task UpdateAsync(Duel? duel);
}