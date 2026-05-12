using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Repositories;

public sealed class AnswersRepository : IAnswersRepository
{
    private readonly QuestionsDbContext _dbContext;

    public AnswersRepository(QuestionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task BulkUploadAsync(IEnumerable<Answer> answers, CancellationToken ct)
    {
        _dbContext.Answers.AddRange(answers);
        
        return _dbContext.SaveChangesAsync(ct);
    }
}