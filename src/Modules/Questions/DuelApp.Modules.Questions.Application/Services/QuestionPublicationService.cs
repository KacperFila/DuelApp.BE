using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Application.Exceptions;
using DuelApp.Modules.Questions.Application.Messages;
using DuelApp.Modules.Questions.Application.Models;
using DuelApp.Modules.Questions.Domain.Questions.Enums;
using DuelApp.Shared.Abstractions.Contexts;
using DuelApp.Shared.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Questions.Application.Services;

public sealed class QuestionPublicationService
{
    private const int BatchSize = 500;

    private readonly IQuestionImportsRepository _questionImportsRepository;
    private readonly IServiceBusMessagePublisher _messagePublisher;
    private readonly IContext _context;
    private readonly ILogger<QuestionPublicationService> _logger;

    public QuestionPublicationService(
        IQuestionImportsRepository questionImportsRepository,
        IServiceBusMessagePublisher messagePublisher,
        IContext context,
        ILogger<QuestionPublicationService> logger)
    {
        _questionImportsRepository = questionImportsRepository;
        _messagePublisher = messagePublisher;
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> RequestPublicationAsync(
        Guid importId,
        CancellationToken cancellationToken)
    {
        var questionImport = await _questionImportsRepository.GetAsync(importId, cancellationToken)
            ?? throw new QuestionImportNotFoundException(importId);

        if (questionImport.Status != ImportStatus.Completed)
        {
            throw new QuestionImportNotCompletedException(importId, questionImport.Status);
        }

        var requestId = Guid.NewGuid();
        var command = new PublishImportedQuestionsMessage(
            requestId,
            importId,
            _context.Identity.UserId,
            _context.RequestId);

        await _messagePublisher.PublishAsync(command, cancellationToken);

        return requestId;
    }

    public async Task<PublishedImportedQuestions> PublishAsync(
        Guid importId,
        CancellationToken cancellationToken)
    {
        var questionsCount = 0;
        var answersCount = 0;

        while (true)
        {
            var publishedBatch = await _questionImportsRepository
                .PublishNextUnpublishedBatchAsync(importId, BatchSize, cancellationToken);

            if (publishedBatch.QuestionsCount == 0)
            {
                break;
            }

            questionsCount += publishedBatch.QuestionsCount;
            answersCount += publishedBatch.AnswersCount;

            _logger.LogInformation(
                "Published {QuestionsCount} questions and {AnswersCount} answers from import {ImportId} in the current batch.",
                publishedBatch.QuestionsCount,
                publishedBatch.AnswersCount,
                importId);
        }

        var publishedQuestions = new PublishedImportedQuestions(
            questionsCount,
            answersCount);

        _logger.LogInformation(
            "Published {QuestionsCount} questions and {AnswersCount} answers from import {ImportId}.",
            publishedQuestions.QuestionsCount,
            publishedQuestions.AnswersCount,
            importId);

        return publishedQuestions;
    }
}
