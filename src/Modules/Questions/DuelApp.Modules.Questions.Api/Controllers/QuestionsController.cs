using DuelApp.Modules.Questions.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DuelApp.Modules.Questions.Api.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    [Authorize]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Upload questions",
        Description = "Uploads a JSON file containing questions and stores them in the database."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Questions uploaded successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid questions file")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<IActionResult> UploadQuestions(
        IFormFile questionsJson,
        IQuestionsService questionsService,
        CancellationToken ct = default)
    {
        await questionsService.UploadQuestionsAsync(questionsJson, ct);

        return Ok(new
        {
            message = "Questions has been uploaded"
        });
    }
    
    [Authorize]
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get questions with answers",
        Description = "Returns a batch of questions together with their possible answers. The amount of questions is provided as a query parameter."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Questions returned successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No questions found")]
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