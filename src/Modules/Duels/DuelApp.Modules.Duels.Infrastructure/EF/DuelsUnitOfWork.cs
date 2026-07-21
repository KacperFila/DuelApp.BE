using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using DuelApp.Shared.Abstractions.Kernel;

namespace DuelApp.Modules.Duels.Infrastructure.EF;

public class DuelsUnitOfWork : IDuelsUnitOfWork
{
    private readonly DuelsDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public DuelsUnitOfWork(
        DuelsDbContext dbContext,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task ExecuteAsync(Func<Task> action, CancellationToken ct)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

        await action();

        var aggregatesWithEvents = _dbContext.ChangeTracker
            .Entries<Duel>()
            .Select(x => x.Entity)
            .Where(x => x.Events.Any())
            .ToList();

        await _dbContext.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        foreach (var aggregate in aggregatesWithEvents)
        {
            var events = aggregate.Events.ToArray();
            await _domainEventDispatcher.DispatchAsync(events);
            aggregate.ClearEvents();
        }
    }
}
