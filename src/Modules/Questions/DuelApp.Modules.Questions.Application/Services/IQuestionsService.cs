using Microsoft.AspNetCore.Http;

namespace DuelApp.Modules.Questions.Application.Services;

public interface IQuestionsService
{
    public Task UploadQuestionsAsync(IFormFile questionsJson, CancellationToken ct);
}