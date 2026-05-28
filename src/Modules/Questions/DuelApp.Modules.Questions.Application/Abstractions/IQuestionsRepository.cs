using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Application.Abstractions;

public interface IQuestionsRepository
{
    public Task BulkUploadAsync(IEnumerable<Question> questions, CancellationToken ct);
    public Task<IEnumerable<Question>> GetQuestionsWithAnswersAsync(int questionsAmount, CancellationToken ct = default);
    public Task<Question?> GetQuestionWithAnswersByIdAsync(Guid questionId, CancellationToken ct = default);
    public Task<bool> CheckAnswerAsync(Guid answerId);
}