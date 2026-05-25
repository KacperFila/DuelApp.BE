namespace DuelApp.Modules.Questions.Shared.Dto;

public record QuestionWithAnswerDto(
    Guid Id,
    string Title,
    List<AnswerDto> Answers
);
