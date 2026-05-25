using DuelApp.Modules.Duels.Domain.Duels.Entities;

namespace DuelApp.Modules.Duels.Application.Abstractions;

public interface IDuelsRepository
{
    public Task<Duel?> GetByIdAsync(Guid duelId);
    public Task<bool> IsPlayerCurrentlyInDuelAsync(Guid playerId);
    public Task CreateDuelAsync(Duel? duel);
    public Task UpdateDuelAsync(Duel duel);
    public Task<Duel?> GetCurrentDuelForPlayerAsync(Guid playerId);
}