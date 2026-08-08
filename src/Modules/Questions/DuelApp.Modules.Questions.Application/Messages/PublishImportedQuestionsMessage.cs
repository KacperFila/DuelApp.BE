using DuelApp.Shared.Abstractions.Messaging;

namespace DuelApp.Modules.Questions.Application.Messages;

public sealed record PublishImportedQuestionsMessage(
    Guid MessageId,
    Guid ImportId,
    Guid RequestedBy,
    string? CorrelationId)
    : IServiceBusMessage
{
    public string Channel => "question-publications";
}
