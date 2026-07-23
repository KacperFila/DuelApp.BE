using DuelApp.Modules.Duels.Domain.Duels.Entities;

namespace DuelApp.Modules.Duels.Application.Abstractions;

public interface IDuelsRepository
{
    public Task<Duel?> GetByIdAsync(Guid duelId);
    public Task<Duel?> GetForUpdateByRoundIdAsync(Guid roundId);
    public Task<bool> IsPlayerCurrentlyInDuelAsync(Guid playerId);
    public Task CreateDuelAsync(Duel? duel);
    public Task UpdateDuelAsync(Duel duel);
    public Task<Guid?> GetCurrentDuelIdForPlayerAsync(Guid playerId);
    public Task<Duel?> GetForUpdateById(Guid duelId);
}
