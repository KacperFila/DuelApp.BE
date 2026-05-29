using System;
using System.Threading.Tasks;
using DuelApp.Modules.Users.Core.Constants;
using DuelApp.Modules.Users.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DuelApp.Modules.Users.Core.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IAvatarStorageService _avatarStorageService;

    public AccountService(
        IUserRepository userRepository, 
        IAvatarStorageService avatarStorageService)
    {
        _userRepository = userRepository;
        _avatarStorageService = avatarStorageService;
    }

    /// <summary>
    /// Uploads a new avatar for the specified user and updates their profile image reference.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="file">The image file to upload as the avatar.</param>
    /// <returns>
    /// A URL pointing to the uploaded avatar image if successful;
    /// otherwise <c>null</c> if the user does not exist.
    /// </returns>
    public async Task<string?> UploadAvatar(Guid userId, IFormFile file)
    {
        if (!IsFileAllowed(file))
        {
            return null;
        }
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }
        
        var blobName = $"users/{user.Id}.png";

        await using var stream = file.OpenReadStream();

        await _avatarStorageService.UploadAsync(blobName, stream, file.ContentType);

        user.ProfileImageKey = blobName;

        await _userRepository.UpdateAsync(user);

        return _avatarStorageService.GetBlobUrl(blobName);
    }

    public string GetUserAvatar(Guid userId)
    {
        return _avatarStorageService.GetAvatarUrl(userId);
    }

    private bool IsFileAllowed(IFormFile file)
    {
        var isValidContentType = UserProfileConstants.AllowedFileContentTypes.Contains(file.ContentType);
        var isFileSizeWithinLimit = file.Length <= UserProfileConstants.MaxFileSizeBytes;

        return isValidContentType && isFileSizeWithinLimit;
    }
}