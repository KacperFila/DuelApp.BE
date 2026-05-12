using DuelApp.Modules.Matchmaking.Application.Abstractions;

namespace DuelApp.Modules.Matchmaking.Infrastructure.EF;

public class MatchmakingUnitOfWork : IMatchmakingUnitOfWork
{
    private readonly MatchmakingDbContext _dbContext;

    public MatchmakingUnitOfWork(MatchmakingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(Func<Task> action, CancellationToken ct)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

        await action();

        await _dbContext.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);
    }
}