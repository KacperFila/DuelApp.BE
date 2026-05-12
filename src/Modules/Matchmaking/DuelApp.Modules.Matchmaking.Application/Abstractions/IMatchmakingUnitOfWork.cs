namespace DuelApp.Modules.Matchmaking.Application.Abstractions;

public interface IMatchmakingUnitOfWork
{
    public Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
}