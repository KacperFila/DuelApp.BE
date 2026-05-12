using DuelApp.Modules.Duels.Domain.Duels.Entities;

namespace DuelApp.Modules.Duels.Application.Abstractions;

public interface IDuelsRepository
{
    public Task<bool> IsPlayerCurrentlyInDuelAsync(Guid playerId);
    public Task CreateDuelAsync(Duel duel);
}