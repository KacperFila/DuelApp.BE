namespace DuelApp.Modules.Matchmaking.Application.Services;

public interface IMatchmakingService
{
    Task JoinQueueAsync(Guid playerId);
    Task LeaveQueueAsync(Guid playerId);
}