using System.Collections.Generic;

namespace DuelApp.Modules.Users.Core.Constants;

public static class UserProfileConstants
{
    public const string DefaultAvatarKey = "default.png";
    public const string ProfilePicturesContainerName = "profile-pictures";
    public static readonly IReadOnlyList<string> AllowedFileContentTypes = ["image/png"];
    public const int MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB
}