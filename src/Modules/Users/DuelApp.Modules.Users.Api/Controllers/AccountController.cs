using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuelApp.Modules.Users.Api.Controllers;

internal class AccountController : BaseController
{
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMe()
    {
        var claims = HttpContext.User.Claims.ToDictionary(c => c.Type, c => c.Value);

        return Ok(claims);
    }
}