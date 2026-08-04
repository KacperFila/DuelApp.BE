using DuelApp.Modules.Questions.Application.Models;

namespace DuelApp.Modules.Questions.Application.Services;

public sealed class QuestionImportValidator
{
    public bool IsQuestionValid(GeneratedQuestion question)
    {
        if (string.IsNullOrWhiteSpace(question.Title) ||
            question.Answers is not { Count: > 0 } ||
            question.Answers.Any(answer => string.IsNullOrWhiteSpace(answer.Content)))
        {
            return false;
        }

        return true;
    }
}
