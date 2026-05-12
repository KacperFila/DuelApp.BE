using System.Text.Json;
using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Application.Exceptions;
using DuelApp.Modules.Questions.Application.Models;
using DuelApp.Modules.Questions.Domain.Questions.Entities;
using DuelApp.Shared.Abstractions.Time;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Questions.Application.Services.Implementations;

public class QuestionsService : IQuestionsService
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly IAnswersRepository _answersRepository;
    private readonly ILogger<QuestionsService> _logger;
    private readonly IClock _clock;
    private readonly List<string> _allowedFileTypes = [".json"];
    
    public QuestionsService(
        IQuestionsRepository questionsRepository,
        IAnswersRepository answersRepository, ILogger<QuestionsService> logger, IClock clock)
    {
        _questionsRepository = questionsRepository;
        _answersRepository = answersRepository;
        _logger = logger;
        _clock = clock;
    }

    public async Task UploadQuestionsAsync(IFormFile questionsJson, CancellationToken ct)
    {
        _logger.LogInformation("Uploading questions started at {DateTime}", _clock.CurrentDate());

        if (!IsValidQuestionsFile(questionsJson))
        {
            throw new InvalidQuestionsJsonFormatException();
        }

        await using var stream = questionsJson.OpenReadStream();

        var questions = new List<Question>();
        var answers = new List<Answer>();

        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<GeneratedQuestionModel>(
                           stream,
                           new JsonSerializerOptions
                           {
                               PropertyNameCaseInsensitive = true
                           },
                           ct))
        {
            if (item is null)
            {
                continue;
            }

            var questionId = Guid.NewGuid();

            var questionAnswers = item.Answers.Select(a => new Answer
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                Content = a.Content,
                IsCorrect = a.IsCorrect
            }).ToList();

            answers.AddRange(questionAnswers);

            questions.Add(new Question
            {
                Id = questionId,
                Title = item.Title,
                AnswerIds = questionAnswers.Select(a => a.Id).ToList()
            });
        }

        await _answersRepository.BulkUploadAsync(answers, ct);
        await _questionsRepository.BulkUploadAsync(questions, ct);

        _logger.LogInformation("Uploading questions finished at {DateTime}", _clock.CurrentDate());
    }

    private bool IsValidQuestionsFile(IFormFile questionsJson)
    {
        return questionsJson is { Length: > 0 }
               && _allowedFileTypes.Contains(Path.GetExtension(questionsJson.FileName));
    }
}