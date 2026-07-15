using DuelApp.Modules.Matchmaking.Application.Services;
using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> StartMatchmaking()
    {
        var userId = Guid.Parse(_context.Identity.KeycloakUserId);
        
        var didMatchmakingStart = await _matchmakingService.TryJoinQueueAsync(userId);
        if (!didMatchmakingStart)
        {
            return Ok(new { message = "User is currently during match or another matchmaking." });
        }

        return Ok(new { message = "MatchmakingStarted" });
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> CancelMatchmaking()
    {
        var userId = Guid.Parse(_context.Identity.KeycloakUserId);
        
        await _matchmakingService.LeaveQueueAsync(userId);

        return Ok();
    }
}