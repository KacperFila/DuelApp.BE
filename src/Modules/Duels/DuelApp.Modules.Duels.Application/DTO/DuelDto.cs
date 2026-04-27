using DuelApp.Modules.Duels.Domain.Duels.Entities.Enums;

namespace DuelApp.Modules.Duels.Application.DTO;

public class DuelDto
{
    public Guid Id { get; set; }

    public Guid PlayerOneId { get; set; }
    public Guid PlayerTwoId { get; set; }

    public int CurrentRound { get; set; }
    public int TotalRounds { get; set; }

    public int PlayerOneScore { get; set; }
    public int PlayerTwoScore { get; set; }

    public Guid WinnerId { get; set; }

    public string Status { get; set; } = DuelStatus.None;
    
    
}