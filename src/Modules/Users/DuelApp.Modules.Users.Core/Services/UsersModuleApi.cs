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

    public async Task<UserInfo?> GetByKeycloakIdAsync(string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        if (user is null)
        {
            return null;
        }
        
        return new UserInfo
        (
            user.Id,
            user.KeycloakUserId,
            user.Email
        );
    }

    public async Task<UserInfo> CreateAsync(string keycloakId, Dictionary<string, IEnumerable<string>> claims)
    {
        claims.TryGetValue(ClaimTypes.Email, out var value);
        var email = value?.SingleOrDefault() ?? string.Empty;
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            KeycloakUserId = keycloakId,
            Email = email,
            ProfileImageKey = UserProfileConstants.DefaultAvatarKey
        };

        await _userRepository.AddAsync(user);
        
        return new UserInfo(
            user.Id,
            email,
            keycloakId
        );
    }
}