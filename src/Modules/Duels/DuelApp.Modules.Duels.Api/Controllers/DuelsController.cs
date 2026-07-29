using DuelApp.Modules.Duels.Api.Models;
using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DuelApp.Modules.Duels.Api.Controllers;

[ApiController]
[Route("api/duel")]
public class DuelsController : ControllerBase
{
    private readonly IDuelsService _duelsService;
    private readonly IContext _context;

    public DuelsController(
        IDuelsService duelsService,
        IContextAccessor contextAccessor)
    {
        _duelsService = duelsService;
        _context = contextAccessor.Current;
    }

    [Authorize]
    [HttpPost("answer")]
    [SwaggerOperation(
        Summary = "Submit answer for current duel round",
        Description = "Submits the authenticated user's answer for a specific duel round."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Answer submitted successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid answer or round data")]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
    {
        var userId = _context.Identity.KeycloakUserId;

        await _duelsService.SubmitAnswerForUserAsync(
            request.AnswerId,
            request.RoundId,
            Guid.Parse(userId));

        return Ok();
    }

    [Authorize]
    [HttpDelete]
    [SwaggerOperation(
        Summary = "Abandon active duel",
        Description = "Removes the authenticated user from their current active duel."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Duel abandoned successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No active duel found")]
    public async Task<IActionResult> AbandonDuel()
    {
        var userId = _context.Identity.KeycloakUserId;

        await _duelsService.AbandonDuelForUserAsync(Guid.Parse(userId));

        return Ok();
    }

    [Authorize]
    [HttpGet("round/current")]
    [SwaggerOperation(
        Summary = "Get current duel round",
        Description = "Returns the current round of the authenticated user's active duel."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Current duel round returned")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No active duel round found")]
    public async Task<IActionResult> GetDuelCurrentRound()
    {
        var userId = _context.Identity.KeycloakUserId;

        var result = await _duelsService.GetCurrentRoundForUserAsync(Guid.Parse(userId));

        return result is not null
            ? Ok(result)
            : NotFound();
    }

    [Authorize]
    [HttpGet("preview")]
    [SwaggerOperation(
        Summary = "Get duel preview",
        Description = "Returns preview information about the authenticated user's current duel."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Duel preview returned")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No active duel found")]
    public async Task<IActionResult> GetDuelPreview()
    {
        var userId = _context.Identity.KeycloakUserId;

        var result = await _duelsService.GetCurrentDuelPreviewAsync(Guid.Parse(userId));

        return result is not null
            ? Ok(result)
            : NotFound();
    }

    [Authorize]
    [HttpGet("current")]
    [SwaggerOperation(
        Summary = "Check active duel status",
        Description = "Checks whether the authenticated user currently participates in an active duel."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Active duel status returned")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<IActionResult> CheckIfUserInActiveDuel()
    {
        var userId = _context.Identity.KeycloakUserId;

        var result = await _duelsService.CheckIfInActiveDuelByUserId(Guid.Parse(userId));

        return Ok(result);
    }
}