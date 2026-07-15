using DuelApp.Modules.Users.Shared.Dto;

namespace DuelApp.Modules.Users.Shared;

public interface IUsersModuleApi
{
    public Task<UserInfo?> GetByKeycloakIdAsync(string keycloakId);
    public Task<UserInfo?> GetById(Guid userId);
    public Task<UserInfo> CreateAsync(string keycloakId, Dictionary<string, IEnumerable<string>> claims);


}