using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Application.Exceptions;
using DuelApp.Modules.Questions.Application.Models;
using DuelApp.Modules.Questions.Domain.Questions.Entities;
using DuelApp.Modules.Questions.Domain.Questions.Enums;
using DuelApp.Shared.Abstractions.Contexts;
using DuelApp.Shared.Abstractions.Time;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Answer = DuelApp.Modules.Questions.Application.Models.Answer;

namespace DuelApp.Modules.Questions.Application.Services.Implementations;

public class QuestionsService : IQuestionsService
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly IQuestionImportsRepository _questionImportsRepository;
    private readonly IQuestionImportFileStorage _questionImportFileStorage;
    private readonly IContext _context;
    private readonly ILogger<QuestionsService> _logger;
    private readonly IClock _clock;
    private readonly List<string> _allowedFileTypes = [".json"];
    
    public QuestionsService(
        IQuestionsRepository questionsRepository,
        IQuestionImportsRepository questionImportsRepository,
        IQuestionImportFileStorage questionImportFileStorage,
        IContext context,
        ILogger<QuestionsService> logger,
        IClock clock)
    {
        _questionsRepository = questionsRepository;
        _questionImportsRepository = questionImportsRepository;
        _questionImportFileStorage = questionImportFileStorage;
        _context = context;
        _logger = logger;
        _clock = clock;
    }

    public async Task<Guid> UploadQuestionsAsync(IFormFile questionsJson, CancellationToken ct)
    {
        _logger.LogInformation("Question import upload started at {DateTime}", _clock.CurrentDate());

        if (!IsValidQuestionsFile(questionsJson))
        {
            throw new InvalidQuestionsJsonFormatException();
        }

        var importId = Guid.NewGuid();
        var blobName = $"imports/{importId:N}/questions.json";

        await using var stream = questionsJson.OpenReadStream();

        var storedFile = await _questionImportFileStorage.UploadAsync(
            stream,
            blobName,
            ct);

        var questionImport = new QuestionImport
        {
            Id = importId,
            BlobName = storedFile.BlobName,
            BlobETag = storedFile.ETag,
            RequestedBy = _context.Identity.UserId,
            Status = ImportStatus.Uploaded,
            CreatedAtUtc = _clock.CurrentDate()
        };

        await _questionImportsRepository.AddAsync(questionImport, ct);

        _logger.LogInformation(
            "Question import {ImportId} uploaded at {DateTime}",
            importId,
            _clock.CurrentDate());

        return importId;
    }

    public async Task<IEnumerable<QuestionWithAnswer>> GetQuestionsWithAnswersBatch(int questionsAmount, CancellationToken ct)
    {
        var questions = await _questionsRepository.GetQuestionsWithAnswersAsync(questionsAmount, ct);

        return questions.Select(x => new QuestionWithAnswer(
        
            x.Id,
            x.Title,
            x.Answers.Select(answer => new Answer
            (
                answer.Id,
                answer.Content,
                answer.IsCorrect
            )).ToList()
        )).ToList();
    }

    private bool IsValidQuestionsFile(IFormFile questionsJson)
    {
        return questionsJson is { Length: > 0 }
               && _allowedFileTypes.Contains(Path.GetExtension(questionsJson.FileName));
    }
}
