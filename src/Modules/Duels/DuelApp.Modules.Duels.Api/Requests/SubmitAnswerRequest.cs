namespace DuelApp.Modules.Duels.Api.Requests;

public sealed record SubmitAnswerRequest(Guid RoundId, Guid AnswerId);
