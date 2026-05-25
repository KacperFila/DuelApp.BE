using DuelApp.Modules.Duels.Domain.Duels.Entities;

namespace DuelApp.Modules.Duels.Application.Services;

public interface IDuelsService
{
    public Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId);
    public Task<Duel?> GetDuelByIdAsync(Guid duelId);
    public Task<bool> CreateNextRoundAsync(Guid duelId, Guid questionId);
    public Task<bool> AbandonDuelAsync();
}