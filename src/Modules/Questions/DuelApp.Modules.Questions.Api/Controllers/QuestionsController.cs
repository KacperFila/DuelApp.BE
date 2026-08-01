using DuelApp.Modules.Questions.Api.Responses;
using DuelApp.Modules.Questions.Application.Models;
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
        Summary = "Start a question import",
        Description = "Stores a JSON file in import storage for asynchronous processing."
    )]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Question import accepted")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid questions file")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<ActionResult<StartQuestionImportResponse>> UploadQuestions(
        IFormFile questionsJson,
        IQuestionsService questionsService,
        CancellationToken ct = default)
    {
        var importId = await questionsService.UploadQuestionsAsync(questionsJson, ct);

        return Accepted(new StartQuestionImportResponse(importId));
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
    public async Task<ActionResult<IEnumerable<QuestionWithAnswer>>> GetQuestionsWithAnswers(
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
