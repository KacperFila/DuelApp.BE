namespace DuelApp.Modules.Users.Shared.Dto;

public record UserInfo(Guid ProfileId, Guid UserId, string Email, string AvatarUri);
