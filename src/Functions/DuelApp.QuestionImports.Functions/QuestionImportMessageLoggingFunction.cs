using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DuelApp.QuestionImports.Functions;

public sealed class QuestionImportMessageLoggingFunction(
    ILogger<QuestionImportMessageLoggingFunction> logger)
{
    [Function(nameof(QuestionImportMessageLoggingFunction))]
    public void Run(
        [ServiceBusTrigger("%QuestionImportsQueueName%", Connection = "QuestionImportsServiceBus")]
        ServiceBusReceivedMessage message)
    {
        logger.LogInformation(
            "Question import message received. MessageId: {MessageId}; SequenceNumber: {SequenceNumber}; DeliveryCount: {DeliveryCount}; BodyLength: {BodyLength}",
            message.MessageId,
            message.SequenceNumber,
            message.DeliveryCount,
            message.Body.ToMemory().Length);
    }
}
