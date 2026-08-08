using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using DuelApp.Shared.Abstractions.Messaging;

namespace DuelApp.Shared.Infrastructure.Messaging.ServiceBus;

public sealed class ServiceBusMessagePublisher : IServiceBusMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public ServiceBusMessagePublisher(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken)
        where TMessage : IServiceBusMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.MessageId == Guid.Empty)
        {
            throw new ArgumentException("Service Bus message ID cannot be empty.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(message.Channel))
        {
            throw new ArgumentException("Service Bus message channel cannot be blank.", nameof(message));
        }

        var serviceBusMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            MessageId = message.MessageId.ToString("N"),
            CorrelationId = message.CorrelationId,
            Subject = typeof(TMessage).FullName
        };

        var sender = _senders.GetOrAdd(message.Channel, _client.CreateSender);

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        var disposeTasks = _senders.Values
            .Select(sender => sender.DisposeAsync().AsTask());

        await Task.WhenAll(disposeTasks);
    }
}
