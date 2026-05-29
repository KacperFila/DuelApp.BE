namespace DuelApp.Modules.Matchmaking.Application.Services;

public interface IMatchmakingService
{
    Task<bool> TryJoinQueueAsync(Guid userId);
    Task LeaveQueueAsync(Guid playerId);
}