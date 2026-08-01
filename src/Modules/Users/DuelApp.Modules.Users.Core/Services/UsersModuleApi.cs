using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DuelApp.Modules.Users.Core.Constants;
using DuelApp.Modules.Users.Core.Entities;
using DuelApp.Modules.Users.Core.Repositories;
using DuelApp.Modules.Users.Shared;
using DuelApp.Modules.Users.Shared.Dto;

namespace DuelApp.Modules.Users.Core.Services;

public class UsersModuleApi : IUsersModuleApi
{
    private readonly IUserRepository _userRepository;
    private readonly IAvatarStorageService _avatarStorageService;
    
    public UsersModuleApi(
        IUserRepository userRepository,
        IAvatarStorageService avatarStorageService)
    {
        _userRepository = userRepository;
        _avatarStorageService = avatarStorageService;
    }

    public async Task<UserInfo?> GetByUserIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByUserIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var avatarUri = await _avatarStorageService.GetAvatarUrlAsync(user.ProfileId);
        
        return new UserInfo
        (
            user.ProfileId,
            user.UserId,
            user.Email,
            avatarUri
        );
    }

    public async Task<UserInfo?> GetByProfileIdAsync(Guid profileId)
    {
        var user = await _userRepository.GetByProfileIdAsync(profileId);
        if (user is null)
        {
            return null;
        }
        
        var avatarUri = await _avatarStorageService.GetAvatarUrlAsync(user.ProfileId);
        
        return new UserInfo
        (
            user.ProfileId,
            user.UserId,
            user.Email,
            avatarUri
        );
    }

    public async Task<UserInfo> CreateAsync(Guid userId, Dictionary<string, IEnumerable<string>> claims)
    {
        claims.TryGetValue(ClaimTypes.Email, out var value);
        var email = value?.SingleOrDefault() ?? string.Empty;
        
        var user = new User
        {
            ProfileId = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            ProfileImageKey = UserProfileConstants.DefaultAvatarKey
        };

        await _userRepository.AddAsync(user);
        
        var avatarUri = await _avatarStorageService.GetAvatarUrlAsync(user.ProfileId);
        
        return new UserInfo(
            user.ProfileId,
            user.UserId,
            user.Email,
            avatarUri
        );
    }
}
