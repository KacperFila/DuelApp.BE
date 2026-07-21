using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Modules.Questions.Shared.Dto;

namespace DuelApp.Modules.Questions.Infrastructure.Services;

public class QuestionsModuleApi : IQuestionsModuleApi
{
    private readonly IQuestionsRepository _questionsRepository;

    public QuestionsModuleApi(IQuestionsRepository questionsRepository)
    {
        _questionsRepository = questionsRepository;
    }

    public async Task<List<QuestionWithAnswerDto>> GetQuestionsWithAnswersAsync(int questionsAmount = 5)
    {
        var questionsWithAnswers = await _questionsRepository.GetQuestionsWithAnswersAsync(questionsAmount);

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

    public async Task<QuestionWithAnswerDto?> GetQuestionWithAnswersByIdAsync(Guid questionId)
    {
        var question = await _questionsRepository.GetQuestionWithAnswersByIdAsync(questionId);

        return question is null
            ? null
            : new QuestionWithAnswerDto
            (
                question.Id,
                question.Title,
                question.Answers.Select(x => new AnswerDto(
                    x.Id,
                    x.Content,
                    x.IsCorrect))
                    .ToList()
            );
    }

    public async Task<bool> CheckAnswerAsync(Guid answerId)
    {
        return await _questionsRepository.CheckAnswerAsync(answerId);
    }
}