using DuelApp.Modules.Duels.Api.Models;
using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> AbandonDuel()
    {
        var userId = _context.Identity.KeycloakUserId;
        await _duelsService.AbandonDuelForUserAsync(Guid.Parse(userId));

        return Ok();
    }
    
    [Authorize]
    [HttpGet("round/current")]
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
    public async Task<IActionResult> GetDuelPreview()
    {
        var userId = _context.Identity.KeycloakUserId;
        var result = await _duelsService.GetCurrentDuelPreviewAsync(Guid.Parse(userId));

        return result is not null
            ? Ok(result)
            : NotFound();
    }
}
