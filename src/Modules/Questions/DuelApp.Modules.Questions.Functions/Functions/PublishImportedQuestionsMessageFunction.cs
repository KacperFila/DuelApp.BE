using System.Text.Json;
using Azure.Messaging.ServiceBus;
using DuelApp.Modules.Questions.Application.Messages;
using DuelApp.Modules.Questions.Application.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Questions.Functions.Functions;

public sealed class PublishImportedQuestionsMessageFunction
{
    private readonly QuestionPublicationService _questionPublicationService;
    private readonly ILogger<PublishImportedQuestionsMessageFunction> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PublishImportedQuestionsMessageFunction(
        QuestionPublicationService questionPublicationService,
        ILogger<PublishImportedQuestionsMessageFunction> logger)
    {
        _questionPublicationService = questionPublicationService;
        _logger = logger;
    }

    [Function(nameof(PublishImportedQuestionsMessageFunction))]
    public async Task Run(
        [ServiceBusTrigger("%QuestionPublicationsQueueName%", Connection = "QuestionPublicationsServiceBus")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        await using var body = message.Body.ToStream();

        var deserializedMessage = await JsonSerializer.DeserializeAsync<PublishImportedQuestionsMessage>(
            body,
            SerializerOptions,
            cancellationToken);

        if (deserializedMessage is null)
        {
            throw new InvalidOperationException(
                $"Service Bus message {message.MessageId} does not contain a publication command.");
        }

        var publishedQuestions = await _questionPublicationService
            .PublishAsync(deserializedMessage.ImportId, cancellationToken);

        _logger.LogInformation(
            "Question publication request {RequestId} for import {ImportId} from user {UserId} completed with {QuestionsCount} questions and {AnswersCount} answers.",
            deserializedMessage.MessageId,
            deserializedMessage.ImportId,
            deserializedMessage.RequestedBy,
            publishedQuestions.QuestionsCount,
            publishedQuestions.AnswersCount);
    }
}
