using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Domain.Duels.Entities;

namespace DuelApp.Modules.Duels.Application.Services;

public interface IDuelsService
{
    public Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId);
    public Task SubmitAnswerAsync(Guid answerId);
    public Task<DuelRoundDto?> GetCurrentRoundAsync();
    public Task<Duel?> GetDuelByIdAsync(Guid duelId);
    public Task<bool> AbandonDuelAsync();
}