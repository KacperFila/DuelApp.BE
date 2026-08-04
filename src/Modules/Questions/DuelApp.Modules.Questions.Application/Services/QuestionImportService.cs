using System.Text.Json;
using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Application.Exceptions;
using DuelApp.Modules.Questions.Application.Mappers;
using DuelApp.Modules.Questions.Domain.Questions.Entities;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Questions.Application.Services;

public sealed class QuestionImportService
{
    private readonly IQuestionImportsRepository _questionImportRepository;
    private readonly IQuestionImportFileStorage _fileStorage;
    private readonly QuestionImportJsonReader _jsonReader;
    private readonly QuestionImportValidator _validator;
    private readonly ILogger<QuestionImportService> _logger;
    private const int BatchSize = 500;

    public QuestionImportService(IQuestionImportsRepository questionImportRepository, IQuestionImportFileStorage fileStorage, QuestionImportJsonReader jsonReader, QuestionImportValidator validator, ILogger<QuestionImportService> logger)
    {
        _questionImportRepository = questionImportRepository;
        _fileStorage = fileStorage;
        _jsonReader = jsonReader;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Imports valid questions from the import blob and saves them in batches.
    /// </summary>
    /// <param name="importId">The import request identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ImportFromBlobAsync(Guid importId, CancellationToken cancellationToken)
    {
        try
        {
            var questionImport = await _questionImportRepository.BeginImportProcessingAsync(importId, cancellationToken);
            if (questionImport is null)
            {
                _logger.LogInformation("Question import {ImportId} is already in a terminal state.", importId);
                return;
            }

            var validQuestionsCount = await ValidateImportFileAsync(questionImport, cancellationToken);
            var alreadySavedQuestionsCount = await SaveQuestionsInBatchesAsync(
                questionImport,
                validQuestionsCount,
                cancellationToken);

            if (alreadySavedQuestionsCount != validQuestionsCount)
            {
                throw new InvalidOperationException(
                    $"Question import {questionImport.Id} has an inconsistent saved questions count. Saved {alreadySavedQuestionsCount} / {validQuestionsCount} valid questions count");
            }

            await _questionImportRepository.CompleteAsync(
                questionImport.Id,
                validQuestionsCount,
                cancellationToken);

            _logger.LogInformation(
                "Question import {ImportId} completed with {QuestionsCount} questions.",
                questionImport.Id,
                validQuestionsCount);
        }
        catch (Exception exception) when (exception is InvalidQuestionImportException or JsonException)
        {
            await _questionImportRepository.FailAsync(importId, exception.Message, cancellationToken);

            _logger.LogWarning(
                exception,
                "Question import {ImportId} failed validation.",
                importId);
        }
    }

    private async Task<int> ValidateImportFileAsync(
        QuestionImport questionImport,
        CancellationToken cancellationToken)
    {
        await using var content = await _fileStorage.OpenReadAsync(
            questionImport.BlobName,
            questionImport.BlobETag,
            cancellationToken);

        var validQuestionsCount = 0;

        await foreach (var question in _jsonReader.ReadAsync(content, cancellationToken))
        {
            if (_validator.IsQuestionValid(question))
            {
                validQuestionsCount++;
            }
        }

        if (validQuestionsCount == 0)
        {
            throw new InvalidQuestionImportException("The import JSON must contain at least one question.");
        }

        return validQuestionsCount;
    }

    private async Task<int> SaveQuestionsInBatchesAsync(
        QuestionImport questionImport,
        int totalQuestions,
        CancellationToken cancellationToken)
    {
        await using var content = await _fileStorage.OpenReadAsync(
            questionImport.BlobName,
            questionImport.BlobETag,
            cancellationToken);

        var questionIndex = 0;
        var alreadySavedQuestionsCount = questionImport.ProcessedQuestionsCount;
        var savedQuestionsCount = alreadySavedQuestionsCount;
        var batch = new List<UnpublishedQuestion>(BatchSize);

        await foreach (var question in _jsonReader.ReadAsync(content, cancellationToken))
        {
            if (IsQuestionAlreadySaved(questionIndex, alreadySavedQuestionsCount))
            {
                questionIndex++;
                continue;
            }

            batch.Add(UnpublishedQuestionMapper.Map(questionImport.Id, questionIndex, question));
            questionIndex++;

            if (batch.Count == BatchSize)
            {
                savedQuestionsCount = await SaveBatchAsync(
                    questionImport.Id,
                    batch,
                    savedQuestionsCount,
                    totalQuestions,
                    cancellationToken);
            }
        }

        if (batch.Count > 0)
        {
            savedQuestionsCount = await SaveBatchAsync(
                questionImport.Id,
                batch,
                savedQuestionsCount,
                totalQuestions,
                cancellationToken);
        }

        return savedQuestionsCount;
    }

    private async Task<int> SaveBatchAsync(
        Guid importId,
        List<UnpublishedQuestion> batch,
        int savedQuestions,
        int totalQuestions,
        CancellationToken cancellationToken)
    {
        savedQuestions += batch.Count;

        await _questionImportRepository.SaveBatchInTransactionAsync(importId, batch, savedQuestions, cancellationToken);

        _logger.LogInformation(
            "Question import {ImportId} saved {SavedQuestions} of {TotalQuestions} questions.",
            importId,
            savedQuestions,
            totalQuestions);

        batch.Clear();

        return savedQuestions;
    }

    private static bool IsQuestionAlreadySaved(int questionIndex, int previouslySavedQuestions)
        => questionIndex < previouslySavedQuestions;

}
