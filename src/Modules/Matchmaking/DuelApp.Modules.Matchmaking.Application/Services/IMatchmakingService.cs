namespace DuelApp.Modules.Matchmaking.Application.Services;

public interface IMatchmakingService
{
    Task<bool> TryJoinQueueAsync();
    Task LeaveQueueAsync(Guid playerId);
    Task TryMatchPlayersAsync(CancellationToken cancellationToken);
}