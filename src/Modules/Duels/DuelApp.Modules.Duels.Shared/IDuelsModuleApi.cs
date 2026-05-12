namespace DuelApp.Modules.Duels.Shared;

public interface IDuelsModuleApi
{
    Task<bool> IsPlayerCurrentlyInDuelAsync(Guid playerId);
}