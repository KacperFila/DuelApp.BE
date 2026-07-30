using System;

namespace DuelApp.Modules.Users.Core.Entities;

public class User
{
    public Guid ProfileId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string? ProfileImageKey { get; set; }
}
