using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.Kernel
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(params IDomainEvent[] events);
    }
}