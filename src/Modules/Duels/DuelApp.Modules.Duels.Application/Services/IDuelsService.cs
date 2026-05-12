namespace DuelApp.Modules.Duels.Application.Services;

public interface IDuelsService
{
    public Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId);
}