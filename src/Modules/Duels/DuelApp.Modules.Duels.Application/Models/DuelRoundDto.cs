namespace DuelApp.Modules.Duels.Application.Models;

public record DuelRoundDto
(
    Guid RoundId,
    int Number,
    int TotalRounds,
    Guid? QuestionId,
    string QuestionText,
    List<AnswerDto> Answers,
    DateTime EndsAt,
    int RoundDurationSeconds
);
