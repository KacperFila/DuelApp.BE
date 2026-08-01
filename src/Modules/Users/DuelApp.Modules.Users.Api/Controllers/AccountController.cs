using System;
using System.Threading.Tasks;
using DuelApp.Modules.Users.Api.Responses;
using DuelApp.Modules.Users.Core.Services;
using DuelApp.Modules.Users.Shared;
using DuelApp.Modules.Users.Shared.Dto;
using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
    [SwaggerOperation(
        Summary = "Get current user profile",
        Description = "Returns the profile information of the currently authenticated user."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "User profile returned successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<ActionResult<UserInfo>> GetMe()
    {
        var context = _contextAccessor.Current;

        var user = await _usersModuleApi.GetByUserIdAsync(context.Identity.UserId);

        return Ok(user);
    }
    
    [HttpGet("me/avatar")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get current user's avatar URL",
        Description = "Returns the avatar URL of the currently authenticated user."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Avatar URL returned successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<ActionResult<string>> GetMyAvatar()
    {
        var context = _contextAccessor.Current;

        var uri = await _accountService.GetUserAvatarAsync(context.Identity.ProfileId);

        return Ok(uri);
    }
    
    [HttpGet("{userId:guid}/avatar")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get user's avatar",
        Description = "Returns the avatar URL for a specific user."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Avatar URL returned successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User avatar not found")]
    public async Task<ActionResult<string>> GetUserAvatar(Guid profileId)
    {
        var uri = await _accountService.GetUserAvatarAsync(profileId);

        return Ok(uri);
    }
    
    [HttpPost("me/avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Upload current user's avatar",
        Description = "Uploads a new avatar image for the currently authenticated user."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Avatar uploaded successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid avatar file")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<ActionResult<UploadAvatarResponse>> Upload(
        IFormFile file,
        IContextAccessor contextAccessor,
        IAccountService accountService)
    {
        var profileId = contextAccessor.Current.Identity.ProfileId;

        var blobName = await accountService.UploadAvatar(profileId, file);

        return blobName is null
            ? BadRequest()
            : Ok(new UploadAvatarResponse(blobName));
    }
}
