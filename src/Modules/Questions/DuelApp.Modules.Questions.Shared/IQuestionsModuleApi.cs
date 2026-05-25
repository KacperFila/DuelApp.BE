using DuelApp.Modules.Questions.Shared.Dto;

namespace DuelApp.Modules.Questions.Shared;

public interface IQuestionsModuleApi
{
    public Task<List<QuestionWithAnswerDto>> GetQuestionsWithAnswersAsync(int questionsAmount = 5);
}