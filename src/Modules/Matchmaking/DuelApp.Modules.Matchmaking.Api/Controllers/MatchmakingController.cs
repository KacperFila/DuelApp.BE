using DuelApp.Modules.Matchmaking.Api.Responses;
using DuelApp.Modules.Matchmaking.Application.Services;
using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DuelApp.Modules.Matchmaking.Api.Controllers;

[ApiController]
[Route("api/matchmaking")]
public class MatchmakingController : ControllerBase
{
    private readonly IMatchmakingService _matchmakingService;
    private readonly IContext _context;
    
    public MatchmakingController(
        IMatchmakingService matchmakingService,
        IContextAccessor contextAccessor)
    {
        _matchmakingService = matchmakingService;
        _context = contextAccessor.Current;
    }
    
    [Authorize]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Start matchmaking",
        Description = "Adds the authenticated user to the matchmaking queue if they are not already in an active match or another matchmaking session."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "User successfully joined matchmaking queue")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<ActionResult<StartMatchmakingResponse>> StartMatchmaking()
    {
        var userId = _context.Identity.UserId;
        
        var didMatchmakingStart = await _matchmakingService.TryJoinQueueAsync(userId);

        if (!didMatchmakingStart)
        {
            return Conflict(new StartMatchmakingResponse(
                "User is currently during match or another matchmaking."));
        }

        return Ok(new StartMatchmakingResponse("MatchmakingStarted"));
    }
    
    [Authorize]
    [HttpDelete]
    [SwaggerOperation(
        Summary = "Cancel matchmaking",
        Description = "Removes the authenticated user from the matchmaking queue."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "User successfully removed from matchmaking queue")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<IActionResult> CancelMatchmaking()
    {
        var userId = _context.Identity.UserId;
        
        await _matchmakingService.LeaveQueueAsync(userId);

        return Ok();
    }
}
