using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Application.Abstractions;

public interface IAnswersRepository
{
    public Task BulkUploadAsync(IEnumerable<Answer> answers, CancellationToken ct);
}