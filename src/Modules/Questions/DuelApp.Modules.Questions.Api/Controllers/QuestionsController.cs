using DuelApp.Modules.Questions.Api.Responses;
using DuelApp.Modules.Questions.Application.Exceptions;
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
    [HttpPost("import/{importId:guid}/publish")]
    [SwaggerOperation(
        Summary = "Queue publication of imported questions",
        Description = "Queues asynchronous publication of completed question imports."
    )]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Question publication queued")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Question import was not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Question import has not completed")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<ActionResult<StartQuestionPublicationResponse>> PublishImportedQuestions(
        Guid importId,
        QuestionPublicationService questionPublicationService,
        CancellationToken ct = default)
    {
        try
        {
            var requestId = await questionPublicationService.RequestPublicationAsync(importId, ct);

            return Accepted(new StartQuestionPublicationResponse(requestId));
        }
        catch (QuestionImportNotFoundException)
        {
            return NotFound();
        }
        catch (QuestionImportNotCompletedException)
        {
            return Conflict();
        }
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
