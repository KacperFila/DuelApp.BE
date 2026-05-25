namespace DuelApp.Modules.Questions.Shared.Dto;

public record AnswerDto(
    Guid Id,
    string Content,
    bool IsCorrect
);