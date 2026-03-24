using DuelApp.Modules.Duels.Application.DTO;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using DuelApp.Modules.Duels.Domain.Duels.Repositories;

namespace DuelApp.Modules.Duels.Application.Services.Implementations;

public class DuelService : IDuelService
{
    private readonly IDuelRepository duelRepository;

    public DuelService(IDuelRepository duelRepository)
    {
        this.duelRepository = duelRepository;
    }

    public async Task<Guid> Create(Guid player1Id, Guid player2Id, int totalRounds)
    {
        var duel = Duel.Create(player1Id, player2Id, totalRounds);

        await duelRepository.AddAsync(duel);

        return duel.Id;
    }

    public async Task<DuelDto> SubmitAnswer(Guid duelId, Guid playerId, bool isCorrect)
    {
        var duel = await duelRepository.GetAsync(duelId);

        duel.SubmitAnswer(playerId, isCorrect);

        await duelRepository.UpdateAsync(duel);

        return Map(duel);
    }

    private static DuelDto Map(Duel duel)
    {
        return new DuelDto
        {
            Id = duel.Id,
            PlayerOneId = duel.PlayerOneId,
            PlayerTwoId = duel.PlayerTwoId,
            CurrentRound = duel.CurrentRound,
            TotalRounds = duel.TotalRounds,
            PlayerOneScore = duel.PlayerOneScore,
            PlayerTwoScore = duel.PlayerTwoScore,
            WinnerId = duel.WinnerId,
            Status = duel.Status
        };
    }
}