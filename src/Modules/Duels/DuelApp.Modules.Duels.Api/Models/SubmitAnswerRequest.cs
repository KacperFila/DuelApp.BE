namespace DuelApp.Modules.Duels.Api.Models;

public record SubmitAnswerRequest
{
    public Guid RoundId { get; }
    public Guid AnswerId { get; }
}
