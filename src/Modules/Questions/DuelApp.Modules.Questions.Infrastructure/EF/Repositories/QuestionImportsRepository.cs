using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Domain.Questions.Entities;

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
}
