using DuelApp.Modules.Questions.Shared;
using DuelApp.Modules.Questions.Shared.Dto;
using Microsoft.EntityFrameworkCore;

namespace DuelApp.Modules.Questions.Infrastructure.Services;

public class QuestionsModuleApi : IQuestionsModuleApi
{
    private readonly QuestionsDbContext _questionsDbContext;

    public QuestionsModuleApi(QuestionsDbContext questionsDbContext)
    {
        _questionsDbContext = questionsDbContext;
    }

    public async Task<List<QuestionWithAnswerDto>> GetQuestionsWithAnswersAsync(int questionsAmount = 5)
    {
        var questionsWithAnswers = await _questionsDbContext
            .Questions
            .OrderBy(x => Microsoft.EntityFrameworkCore.EF.Functions.Random())
            .Take(questionsAmount)
            .Include(x => x.Answers)
            .ToListAsync();

        return questionsWithAnswers.Select(x => new QuestionWithAnswerDto(
            Id: x.Id,
            Title: x.Title,
            Answers: x.Answers.Select(a => new AnswerDto(
                Id: a.Id,
                Content: a.Content,
                IsCorrect: a.IsCorrect
            )).ToList()
        )).ToList();
    }
}