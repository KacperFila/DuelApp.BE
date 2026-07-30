using DuelApp.Modules.Users.Shared.Dto;

namespace DuelApp.Modules.Users.Shared;

public interface IUsersModuleApi
{
    public Task<UserInfo?> GetByUserIdAsync(Guid userId);
    public Task<UserInfo?> GetByProfileIdAsync(Guid profileId);
    public Task<UserInfo> CreateAsync(Guid userId, Dictionary<string, IEnumerable<string>> claims);
}
