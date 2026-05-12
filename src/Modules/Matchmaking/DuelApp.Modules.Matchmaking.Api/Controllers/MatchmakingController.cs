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
    
    public MatchmakingController(IMatchmakingService matchmakingService)
    {
        _matchmakingService = matchmakingService;
    }
    
    [Authorize]
    [HttpPost("start")]
    public async Task<IActionResult> StartMatchmaking()
    {
        var didMatchmakingStart = await _matchmakingService.TryJoinQueueAsync();
        if (!didMatchmakingStart)
        {
            return Ok(new { message = "User is currently during match or another matchmaking." });
        }

        return Ok(new { message = "MatchmakingStarted" });
    }
}