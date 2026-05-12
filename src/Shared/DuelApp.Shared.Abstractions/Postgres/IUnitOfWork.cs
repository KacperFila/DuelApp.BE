using System;
using System.Threading;
using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.Postgres;

public interface IUnitOfWork<TContext>
{
    Task ExecuteAsync(Func<Task> action, CancellationToken ct);
}