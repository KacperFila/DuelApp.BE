using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Application.Abstractions;

public interface IQuestionsRepository
{
    public Task BulkUploadAsync(IEnumerable<Question> questions, CancellationToken ct);
}