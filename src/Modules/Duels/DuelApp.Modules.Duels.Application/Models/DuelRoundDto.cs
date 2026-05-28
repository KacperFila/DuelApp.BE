namespace DuelApp.Modules.Duels.Application.Models;

public record DuelRoundDto
(
    int Number,
    Guid? QuestionId,
    string QuestionText,
    List<AnswerDto> Answers
);