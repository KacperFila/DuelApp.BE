using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Shared;

namespace DuelApp.Modules.Duels.Infrastructure.Services;

public class DuelsModuleApi : IDuelsModuleApi
{
    private readonly IDuelsRepository _duelsRepository;
    
    public DuelsModuleApi(DuelsDbContext dbContext, IDuelsRepository duelsRepository)
    {
        _duelsRepository = duelsRepository;
    }

    public async Task<bool> IsPlayerCurrentlyInDuelAsync(Guid playerId)
    {
       return await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerId);
    }
}