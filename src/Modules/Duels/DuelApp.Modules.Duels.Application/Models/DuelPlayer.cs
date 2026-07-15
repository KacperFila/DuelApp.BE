namespace DuelApp.Modules.Duels.Application.Models;

public class DuelPlayer
{
    public string Email { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public string AvatarUri { get; set; } = string.Empty;
}