namespace DuelApp.Modules.Duels.Application.Models;

public class DuelPreview
{
    public DuelPlayer Player { get; set; } = new();
    public DuelPlayer Opponent { get; set; } = new();
}