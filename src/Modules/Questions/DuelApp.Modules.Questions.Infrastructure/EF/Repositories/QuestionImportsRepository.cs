using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Domain.Questions.Entities;
using DuelApp.Modules.Questions.Domain.Questions.Enums;
using DuelApp.Modules.Questions.Infrastructure.EF.Mappers;
using Microsoft.EntityFrameworkCore;
using PublishedImportedQuestions = DuelApp.Modules.Questions.Application.Models.PublishedImportedQuestions;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Repositories;

public sealed class QuestionImportsRepository : IQuestionImportsRepository
{
    private readonly QuestionsDbContext _dbContext;

    public QuestionImportsRepository(QuestionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        QuestionImport questionImport,
        CancellationToken cancellationToken)
    {
        await _dbContext.QuestionImports.AddAsync(questionImport, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<QuestionImport?> GetAsync(
        Guid importId,
        CancellationToken cancellationToken)
    {
        return _dbContext.QuestionImports
            .AsNoTracking()
            .SingleOrDefaultAsync(questionImport => questionImport.Id == importId, cancellationToken);
    }

    public async Task<QuestionImport?> BeginImportProcessingAsync(
        Guid importId,
        CancellationToken cancellationToken)
    {
        var questionImport = await _dbContext.QuestionImports
            .SingleOrDefaultAsync(x => x.Id == importId, cancellationToken);

        if (questionImport is null)
        {
            throw new InvalidOperationException($"Question import {importId} does not exist yet.");
        }

        if (questionImport.Status is ImportStatus.Completed or ImportStatus.Failed)
        {
            return null;
        }

        questionImport.Status = ImportStatus.Processing;
        questionImport.ErrorMessage = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return questionImport;
    }

    public async Task SaveBatchInTransactionAsync(
        Guid importId,
        IReadOnlyCollection<UnpublishedQuestion> questions,
        int processedQuestionsCount,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await AddQuestionsAsync(questions, cancellationToken);
            await UpdateProcessedQuestionsCountAsync(importId, processedQuestionsCount, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            // Release entities from the completed batch; otherwise EF tracks every earlier batch
            // and change detection can slow down subsequent SaveChanges calls.
            _dbContext.ChangeTracker.Clear();
        }
    }

    public async Task CompleteAsync(
        Guid importId,
        int totalQuestionsCount,
        CancellationToken cancellationToken)
    {
        var questionImport = await _dbContext.QuestionImports
            .SingleAsync(x => x.Id == importId, cancellationToken);

        questionImport.Status = ImportStatus.Completed;
        questionImport.TotalQuestionsCount = totalQuestionsCount;
        questionImport.RejectedQuestionsCount = 0;
        questionImport.CompletedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task FailAsync(
        Guid importId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var questionImport = await _dbContext.QuestionImports
            .SingleOrDefaultAsync(x => x.Id == importId, cancellationToken);

        if (questionImport is null || questionImport.Status == ImportStatus.Completed)
        {
            return;
        }

        questionImport.Status = ImportStatus.Failed;
        questionImport.ErrorMessage = errorMessage.Length <= 4000
            ? errorMessage
            : errorMessage[..4000];
        questionImport.CompletedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PublishedImportedQuestions> PublishNextUnpublishedBatchAsync(
        Guid importId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var unpublishedQuestionsAndAnswers = await _dbContext.UnpublishedQuestions
            .Include(question => question.Answers)
            .Where(question => question.QuestionImportId == importId
                && question.QuestionImport.Status == ImportStatus.Completed)
            .OrderBy(question => question.SourcePosition)
            .ThenBy(question => question.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (unpublishedQuestionsAndAnswers.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return new PublishedImportedQuestions(0, 0);
        }

        var questionsToBePublished = unpublishedQuestionsAndAnswers
            .Select(QuestionPublicationMapper.Map)
            .ToList();

        _dbContext.Questions.AddRange(questionsToBePublished);
        _dbContext.UnpublishedQuestions.RemoveRange(unpublishedQuestionsAndAnswers);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PublishedImportedQuestions(
            QuestionsCount: questionsToBePublished.Count,
            AnswersCount: questionsToBePublished.Sum(question => question.Answers.Count));
    }

    private Task AddQuestionsAsync(
        IReadOnlyCollection<UnpublishedQuestion> questions,
        CancellationToken cancellationToken)
        => _dbContext.UnpublishedQuestions.AddRangeAsync(questions, cancellationToken);

    private async Task UpdateProcessedQuestionsCountAsync(
        Guid importId,
        int processedQuestionsCount,
        CancellationToken cancellationToken)
    {
        var questionImport = await _dbContext.QuestionImports
            .SingleAsync(x => x.Id == importId, cancellationToken);

        questionImport.ProcessedQuestionsCount = processedQuestionsCount;
    }
}
