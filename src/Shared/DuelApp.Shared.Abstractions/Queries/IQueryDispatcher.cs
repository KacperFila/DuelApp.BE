using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.Queries
{
    public interface IQueryDispatcher
    {
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
    }
}