using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Application.Abstractions;

public interface IQuestionImportsRepository
{
    Task AddAsync(QuestionImport questionImport, CancellationToken cancellationToken);
}
