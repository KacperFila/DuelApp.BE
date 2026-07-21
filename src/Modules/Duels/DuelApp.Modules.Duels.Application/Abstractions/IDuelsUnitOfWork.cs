namespace DuelApp.Modules.Duels.Application.Abstractions;

public interface IDuelsUnitOfWork
{
    public Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
}