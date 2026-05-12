using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Duels.Application.Services;

public class DuelsService : IDuelsService
{
    private readonly IDuelsRepository _duelsRepository;
    private readonly ILogger<DuelsService> _logger;
    private const int RoundsCount = 5;

    public DuelsService(
        IDuelsRepository duelsRepository,
        ILogger<DuelsService> logger)
    {
        _duelsRepository = duelsRepository;
        _logger = logger;
    }
    
    public async Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId)
    {
        if (await AnyPlayerCurrentlyInDuel(playerOneId, playerTwoId))
        {
            _logger.LogWarning("New duel cannot be created for players: {Player1} and {Player2}- at least one of the players is currently in duel.",
                playerOneId,
                playerTwoId);
            
            return null;
        }
        
        var duel = Duel.Create(playerOneId, playerTwoId, RoundsCount);
        await _duelsRepository.CreateDuelAsync(duel);
        
        return duel.Id;
    }

    private async Task<bool> AnyPlayerCurrentlyInDuel(Guid playerOneId, Guid playerTwoId)
    {
        return await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerOneId)
            || await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerTwoId);
    }
}