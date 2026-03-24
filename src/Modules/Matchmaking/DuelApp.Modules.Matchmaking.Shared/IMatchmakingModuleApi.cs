using DuelApp.Modules.Matchmaking.Shared.DTO;

namespace DuelApp.Modules.Matchmaking.Shared;

public interface IMatchmakingModuleApi
{
    /// <summary>
    /// Adds a player to the matchmaking queue. Returns a MatchmakingResult if another player is found.
    /// </summary>
    MatchmakingResultDto? JoinQueue(Guid userId);
}