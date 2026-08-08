using DuelApp.Modules.Questions.Domain.Questions.Entities;

namespace DuelApp.Modules.Questions.Infrastructure.EF.Mappers;

internal static class QuestionPublicationMapper
{
    public static Question Map(UnpublishedQuestion unpublishedQuestion)
    {
        var answers = unpublishedQuestion.Answers
            .OrderBy(answer => answer.SourcePosition)
            .Select(answer => new Answer
            {
                Id = answer.Id,
                QuestionId = unpublishedQuestion.Id,
                Content = answer.Content,
                IsCorrect = answer.IsCorrect
            })
            .ToList();

        return new Question
        {
            Id = unpublishedQuestion.Id,
            Title = unpublishedQuestion.Title,
            AnswerIds = unpublishedQuestion.AnswerIds.ToList(),
            Answers = answers
        };
    }
}
