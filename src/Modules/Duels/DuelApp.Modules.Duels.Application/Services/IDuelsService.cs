using DuelApp.Modules.Duels.Application.Models;

namespace DuelApp.Modules.Duels.Application.Services;

public interface IDuelsService
{
    public Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId);
    public Task StartDuelAsync(Guid duelId);
    public Task SubmitAnswerForUserAsync(Guid answerId, Guid roundId, Guid userId);
    public Task<DuelRoundDto?> GetCurrentRoundForUserAsync(Guid userId);
    public Task AbandonDuelForUserAsync(Guid userId);
    public Task<DuelPreview?> GetCurrentDuelPreviewAsync(Guid userId);
    public Task ExpireCurrentRoundAsync(Guid roundId);
}
