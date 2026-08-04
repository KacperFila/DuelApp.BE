using System.Text.Json;
using Azure.Messaging.ServiceBus;
using DuelApp.Modules.Questions.Application.Services;
using DuelApp.Modules.Questions.Functions.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Questions.Functions.Functions;

public sealed class QuestionImportMessageFunction
{
    private const string BlobCreatedEventType = "Microsoft.Storage.BlobCreated";
    private const string SubjectPrefix = "/blobServices/default/containers/question-imports/blobs/imports/";
    private const string SubjectSuffix = "/questions.json";
    private readonly QuestionImportService _questionImportService;
    private readonly ILogger<QuestionImportMessageFunction> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QuestionImportMessageFunction(
        QuestionImportService questionImportService,
        ILogger<QuestionImportMessageFunction> logger)
    {
        _questionImportService = questionImportService;
        _logger = logger;
    }

    [Function(nameof(QuestionImportMessageFunction))]
    public async Task Run(
        [ServiceBusTrigger("%QuestionImportsQueueName%", Connection = "QuestionImportsServiceBus")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        await using var body = message.Body.ToStream();

        var eventGridEvent = await JsonSerializer.DeserializeAsync<BlobCreatedEvent>(
            body,
            SerializerOptions,
            cancellationToken);

        if (eventGridEvent is null || !EventTypeIsBlobCreated(eventGridEvent))
        {
            _logger.LogWarning("Ignoring Service Bus message {MessageId} because it is not a BlobCreated Event Grid event.", message.MessageId);
            return;
        }

        if (!TryGetImportId(eventGridEvent.Subject, out var importId))
        {
            _logger.LogWarning("Ignoring BlobCreated event {MessageId} with an unsupported subject {Subject}.", message.MessageId, eventGridEvent.Subject);
            return;
        }

        await _questionImportService.ImportFromBlobAsync(importId, cancellationToken);
    }

    private static bool EventTypeIsBlobCreated(BlobCreatedEvent eventGridEvent)
        => string.Equals(eventGridEvent.EventType, BlobCreatedEventType, StringComparison.Ordinal);

    private static bool TryGetImportId(string? subject, out Guid importId)
    {
        importId = Guid.Empty;

        if (subject is null
            || !subject.StartsWith(SubjectPrefix, StringComparison.OrdinalIgnoreCase)
            || !subject.EndsWith(SubjectSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var importIdValue = subject[SubjectPrefix.Length..^SubjectSuffix.Length];

        return Guid.TryParseExact(importIdValue, "N", out importId);
    }
}
