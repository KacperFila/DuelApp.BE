namespace DuelApp.Modules.Questions.Application.Models;

public record Answer(
    Guid Id,
    string Content,
    bool IsCorrect
);