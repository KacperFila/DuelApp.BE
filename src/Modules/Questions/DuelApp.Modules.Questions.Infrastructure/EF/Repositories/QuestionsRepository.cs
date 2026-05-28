using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Domain.Questions.Entities;
using Microsoft.EntityFrameworkCore;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Repositories;

public class QuestionsRepository : IQuestionsRepository
{
    private readonly QuestionsDbContext _dbContext;

    public QuestionsRepository(QuestionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task BulkUploadAsync(IEnumerable<Question> questions, CancellationToken ct)
    {
        _dbContext.Questions.AddRange(questions);
        
        return _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Question>> GetQuestionsWithAnswersAsync(int questionsAmount, CancellationToken ct = default)
    {
        var questionsWithAnswers = await _dbContext
            .Questions
            .OrderBy(x => Microsoft.EntityFrameworkCore.EF.Functions.Random())
            .Take(questionsAmount)
            .Include(x => x.Answers)
            .ToListAsync(cancellationToken: ct);

        return questionsWithAnswers;
    }

    public async Task<Question?> GetQuestionWithAnswersByIdAsync(Guid questionId, CancellationToken ct = default)
    {
        return await _dbContext
            .Questions
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == questionId, ct);
    }

    public async Task<bool> CheckAnswerAsync(Guid answerId)
    {
        var answer = await _dbContext
            .Answers
            .Select(x => new
            {
                x.Id,
                x.IsCorrect
            })
            .FirstOrDefaultAsync(x => x.Id == answerId);
        
        return answer?.IsCorrect ?? false;
    }
}