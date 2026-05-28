using DuelApp.Modules.Questions.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DuelApp.Modules.Questions.Api.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UploadQuestions(
        IFormFile questionsJson,
        IQuestionsService questionsService,
        CancellationToken ct = default)
    {
        await questionsService.UploadQuestionsAsync(questionsJson, ct);
        return Ok(new { message = "Messages has been uploaded" });
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetQuestionsWithAnswers(
        [FromQuery] int questionsAmount,
        IQuestionsService questionsService,
        CancellationToken ct = default)
    {
        var questions = await questionsService.GetQuestionsWithAnswersBatch(questionsAmount, ct);
        
        return questions.Any()
            ? Ok(questions)
            : NotFound();
    }
}