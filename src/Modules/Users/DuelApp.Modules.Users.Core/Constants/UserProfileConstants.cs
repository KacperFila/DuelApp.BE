using System.Collections.Generic;

namespace DuelApp.Modules.Users.Core.Constants;

public static class UserProfileConstants
{
    public static readonly string DefaultAvatarKey = "default.png";
    public static readonly string ProfilePicturesContainerName = "profile-pictures";
    public static readonly List<string> AllowedFileContentTypes = ["image/png"];
    public static readonly int MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB
}