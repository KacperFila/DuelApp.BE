using System;

namespace DuelApp.Modules.Users.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string KeycloakUserId { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string? ProfileImageKey { get; set; }
}