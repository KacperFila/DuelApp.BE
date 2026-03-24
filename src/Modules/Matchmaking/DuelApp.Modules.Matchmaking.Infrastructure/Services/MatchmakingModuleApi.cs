using DuelApp.Modules.Matchmaking.Application.Services;
using DuelApp.Modules.Matchmaking.Shared;
using DuelApp.Modules.Matchmaking.Shared.DTO;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Matchmaking.Infrastructure.Services;

public class MatchmakingModuleApi : IMatchmakingModuleApi
{
    private readonly IMatchmakingManager matchmakingManager;
    private readonly ILogger<MatchmakingModuleApi> logger;
    
    public MatchmakingModuleApi(IMatchmakingManager matchmakingManager, ILogger<MatchmakingModuleApi> logger)
    {
        this.matchmakingManager = matchmakingManager;
        this.logger = logger;
    }

    public MatchmakingResultDto? JoinQueue(Guid userId)
    {
        logger.LogInformation("User with id: {UserId} joined queue", userId);
        var matchmakingResult =  matchmakingManager.JoinQueue(userId);

        var result = matchmakingResult is not null
            ? new MatchmakingResultDto(matchmakingResult.Player1, matchmakingResult.Player2)
            : null;
        
        return result;
    }
}