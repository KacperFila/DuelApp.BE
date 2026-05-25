using DuelApp.Modules.Duels.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuelApp.Modules.Duels.Api.Controllers;

[ApiController]
[Route("api/duel")]
public class DuelsController : ControllerBase
{
    private readonly IDuelsService _duelsService;

    public DuelsController(IDuelsService duelsService)
    {
        _duelsService = duelsService;
    }

    [Authorize]
    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer()
    {
        var result = await _duelsService.AbandonDuelAsync();

        return result ? Ok() : NotFound();
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> AbandonDuel()
    {
        var result = await _duelsService.AbandonDuelAsync();

        return result ? Ok() : NotFound();
    }
}