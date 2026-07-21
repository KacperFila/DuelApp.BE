using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Domain.Duels.Entities;

namespace DuelApp.Modules.Duels.Application.Services;

public interface IDuelsService
{
    public Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId);
    public Task SubmitAnswerForUserAsync(Guid? answerId, Guid userId);
    public Task<DuelRoundDto?> GetCurrentRoundForUserAsync(Guid userId);
    public Task<Duel?> GetDuelByIdAsync(Guid duelId);
    public Task AbandonDuelForUserAsync(Guid userId);
    public Task<DuelPreview?> GetCurrentDuelPreviewAsync(Guid userId);
}