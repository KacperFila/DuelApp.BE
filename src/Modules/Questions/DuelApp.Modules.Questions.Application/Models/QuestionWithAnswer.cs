namespace DuelApp.Modules.Questions.Application.Models;

public record QuestionWithAnswer(
    Guid Id,
    string Title,
    List<Answer> Answers
);