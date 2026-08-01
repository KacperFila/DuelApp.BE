using System;
using System.Linq;
using System.Threading.Tasks;
using DuelApp.Modules.Users.Core.Constants;
using DuelApp.Modules.Users.Core.Repositories;
using Microsoft.AspNetCore.Http;

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
    /// Uploads a new avatar image for a user, stores it in blob storage,
    /// and updates the user's profile image reference.
    /// </summary>
    /// <param name="profileId">
    /// The unique identifier of the user whose avatar is being uploaded.
    /// </param>
    /// <param name="file">
    /// The image file to upload as the user's avatar.
    /// </param>
    /// <returns>
    /// The URL of the uploaded avatar image if the upload succeeds;
    /// otherwise, <c>null</c> if the file is invalid or the user does not exist.
    /// </returns>
    public async Task<string?> UploadAvatar(Guid profileId, IFormFile file)
    {
        if (!IsFileAllowed(file))
        {
            return null;
        }
        
        var user = await _userRepository.GetByProfileIdAsync(profileId);
        if (user is null)
        {
            return null;
        }
        
        var blobName = $"users/{user.ProfileId}.png";

        await using var stream = file.OpenReadStream();

        await _avatarStorageService.UploadAsync(blobName, stream, file.ContentType);

        user.ProfileImageKey = blobName;

        await _userRepository.UpdateAsync(user);

        return _avatarStorageService.GetBlobUrl(blobName);
    }

    /// <summary>
    /// Retrieves the avatar URL for the specified user.
    /// </summary>
    /// <param name="profileId">
    /// The unique identifier of the user whose avatar URL should be retrieved.
    /// </param>
    /// <returns>
    /// A URL pointing to the user's avatar image.
    /// </returns>
    public Task<string> GetUserAvatarAsync(Guid profileId)
    {
        return _avatarStorageService.GetAvatarUrlAsync(profileId);
    }

    /// <summary>
    /// Determines whether the uploaded file meets the allowed avatar requirements.
    /// </summary>
    /// <param name="file">
    /// The uploaded file to validate.
    /// </param>
    /// <returns>
    /// <c>true</c> if the file content type and size are allowed;
    /// otherwise, <c>false</c>.
    /// </returns>
    private bool IsFileAllowed(IFormFile file)
    {
        var isValidContentType = UserProfileConstants.AllowedFileContentTypes.Contains(file.ContentType);
        var isFileSizeWithinLimit = file.Length <= UserProfileConstants.MaxFileSizeBytes;

        return isValidContentType && isFileSizeWithinLimit;
    }
}
