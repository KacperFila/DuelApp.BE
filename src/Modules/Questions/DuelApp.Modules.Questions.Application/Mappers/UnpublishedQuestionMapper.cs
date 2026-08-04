using DuelApp.Modules.Questions.Application.Models;
using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Application.Mappers;

internal static class UnpublishedQuestionMapper
{
    public static UnpublishedQuestion Map(
        Guid importId,
        int sourcePosition,
        GeneratedQuestion question)
    {
        var unpublishedQuestionId = Guid.NewGuid();
        var answers = question.Answers.Select((answer, answerPosition) => new UnpublishedAnswer
        {
            Id = Guid.NewGuid(),
            UnpublishedQuestionId = unpublishedQuestionId,
            SourcePosition = answerPosition,
            Content = answer.Content.Trim(),
            IsCorrect = answer.IsCorrect
        }).ToList();

        return new UnpublishedQuestion
        {
            Id = unpublishedQuestionId,
            QuestionImportId = importId,
            SourcePosition = sourcePosition,
            Title = question.Title.Trim(),
            AnswerIds = answers.Select(answer => answer.Id).ToList(),
            Answers = answers
        };
    }
}
