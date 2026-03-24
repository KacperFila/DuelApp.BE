using DuelApp.Modules.Duels.Application.DTO;

namespace DuelApp.Modules.Duels.Application.Services;

public interface IDuelService
{
    Task<Guid> Create(Guid player1Id, Guid player2Id, int totalRounds);

    Task<DuelDto> SubmitAnswer(Guid duelId, Guid playerId, bool isCorrect);
}