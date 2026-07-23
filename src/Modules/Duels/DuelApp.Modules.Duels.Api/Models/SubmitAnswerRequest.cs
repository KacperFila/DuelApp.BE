namespace DuelApp.Modules.Duels.Api.Models;

public record SubmitAnswerRequest(Guid RoundId, Guid AnswerId);