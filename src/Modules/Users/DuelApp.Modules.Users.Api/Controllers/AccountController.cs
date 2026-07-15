using System;
using System.Threading.Tasks;
using DuelApp.Modules.Users.Core.Services;
using DuelApp.Modules.Users.Shared;
using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DuelApp.Modules.Users.Api.Controllers;

[ApiController]
[Route("api/users")]
internal class AccountController : ControllerBase
{
    private readonly IUsersModuleApi _usersModuleApi;
    private readonly IContextAccessor _contextAccessor;
    private readonly IAccountService _accountService;
    public AccountController(
        IUsersModuleApi usersModuleApi,
        IContextAccessor contextAccessor, 
        IAccountService accountService)
    {
        _usersModuleApi = usersModuleApi;
        _contextAccessor = contextAccessor;
        _accountService = accountService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var context = _contextAccessor.Current;

        var user = await _usersModuleApi.GetByKeycloakIdAsync(
            context.Identity.KeycloakUserId);

        return Ok(user);
    }
    
    [HttpGet("me/avatar")]
    [Authorize]
    public IActionResult GetMyAvatar()
    {
        var context = _contextAccessor.Current;

        var uri = _accountService.GetUserAvatar(context.Identity.Id);

        return Ok(uri);
    }
    
    [HttpGet("{userId:guid}/avatar")]
    [Authorize]
    public IActionResult GetUserAvatar(Guid userId)
    {
        var uri = _accountService.GetUserAvatar(userId);

        return Ok(uri);
    }
    
    [HttpPost("me/avatar")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        IContextAccessor contextAccessor,
        IAccountService accountService)
    {
        var userId = contextAccessor.Current.Identity.Id;

        var blobName = await accountService.UploadAvatar(userId, file);

        return blobName is null
            ? BadRequest()
            : Ok(new
            {
                profileImageUrl = blobName
            });
    }
}