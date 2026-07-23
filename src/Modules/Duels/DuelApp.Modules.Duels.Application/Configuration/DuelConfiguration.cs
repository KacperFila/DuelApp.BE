using System.ComponentModel.DataAnnotations;

namespace DuelApp.Modules.Duels.Application.Configuration;

public record DuelConfiguration
{
    public const string SectionName = "duels:DuelConfiguration";
    
    [Range(1, 100)]
    public int DuelRoundCount { get; init; }
    
    [Range(1, 600)]
    public int RoundDurationSeconds { get; init; }
}