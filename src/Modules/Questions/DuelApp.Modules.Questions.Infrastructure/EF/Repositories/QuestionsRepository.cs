using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Domain.Questions.Entities;

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
}