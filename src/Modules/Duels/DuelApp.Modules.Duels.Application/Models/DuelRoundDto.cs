namespace DuelApp.Modules.Duels.Application.Models;

public record DuelRoundDto
(
    int Number,
    int TotalRounds,
    Guid? QuestionId,
    string QuestionText,
    List<AnswerDto> Answers
);