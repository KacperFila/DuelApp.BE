using System;

namespace DuelApp.Shared.Abstractions.Messaging;

public interface IServiceBusMessage
{
    Guid MessageId { get; }
    string? CorrelationId { get; }
    string Channel { get; }
}
