using System;
using System.Threading.Tasks;

namespace DuelApp.Shared.Infrastructure.Postgres
{
    public interface IUnitOfWork
    {
        Task ExecuteAsync(Func<Task> action);
    }
}