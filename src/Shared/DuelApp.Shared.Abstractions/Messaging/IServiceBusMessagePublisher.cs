using System.Threading;
using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.Messaging;

public interface IServiceBusMessagePublisher
{
    Task PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken)
        where TMessage : IServiceBusMessage;
}
