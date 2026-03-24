using DuelApp.Modules.Matchmaking.Application.DTO;

namespace DuelApp.Modules.Matchmaking.Application.Services;

public interface IMatchmakingManager
{
    public MatchmakingResultDto? JoinQueue(Guid userId);
}