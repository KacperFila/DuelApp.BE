using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Application.Abstractions;

public interface IQuestionImportsRepository
{
    Task AddAsync(QuestionImport questionImport, CancellationToken cancellationToken);
    Task<QuestionImport?> BeginImportProcessingAsync(Guid importId, CancellationToken cancellationToken);
    Task SaveBatchInTransactionAsync(
        Guid importId,
        IReadOnlyCollection<UnpublishedQuestion> questions,
        int processedQuestionsCount,
        CancellationToken cancellationToken);
    Task CompleteAsync(Guid importId, int totalQuestionsCount, CancellationToken cancellationToken);
    Task FailAsync(Guid importId, string errorMessage, CancellationToken cancellationToken);
}
