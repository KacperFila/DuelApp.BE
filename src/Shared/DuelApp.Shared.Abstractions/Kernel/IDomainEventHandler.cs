using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.Kernel
{
    public interface IDomainEventHandler<in TEvent> where TEvent : class, IDomainEvent
    {
        Task HandleAsync(TEvent @event);
    }
}