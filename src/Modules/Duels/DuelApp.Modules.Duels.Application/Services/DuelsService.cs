using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Exceptions;
using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using DuelApp.Modules.Duels.Domain.Duels.ValueObjects;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Shared.Abstractions.Contexts;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Duels.Application.Services;

public class DuelsService : IDuelsService
{
    private readonly IDuelsRepository _duelsRepository;
    private readonly ILogger<DuelsService> _logger;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IQuestionsModuleApi _questionsModuleApi;
    private readonly IContext _context;
    private const int RoundsCount = 5;

    public DuelsService(
        IDuelsRepository duelsRepository,
        ILogger<DuelsService> logger,
        IRealTimeNotifier realTimeNotifier,
        IContext context,
        IQuestionsModuleApi questionsModuleApi)
    {
        _duelsRepository = duelsRepository;
        _logger = logger;
        _realTimeNotifier = realTimeNotifier;
        _context = context;
        _questionsModuleApi = questionsModuleApi;
    }
    
    public async Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId)
    {
        if (await AnyPlayerCurrentlyInDuel(playerOneId, playerTwoId))
        {
            _logger.LogWarning("New duel cannot be created for players: {Player1} and {Player2}- at least one of the players is currently in duel.",
                playerOneId,
                playerTwoId);
            
            return null;
        }
        
        var questions = await _questionsModuleApi.GetQuestionsWithAnswersAsync(RoundsCount);

        var rounds = new List<DuelRound>();
        foreach (var (question, index) in questions.Select((value, index) => (value, index)))
        {
            rounds.Add(DuelRound.Create(index + 1, question.Id));
        }
        
        var duel = Duel.Create(playerOneId, playerTwoId, rounds);
        await _duelsRepository.CreateDuelAsync(duel);
        
        return duel.Id;
    }

    public async Task<Duel?> GetDuelByIdAsync(Guid duelId)
    {
        var duel = await _duelsRepository.GetByIdAsync(duelId);
        if (duel is null)
        {
            _logger.LogWarning("No duel with id {DuelId} found", duelId);
            return null;
        }
        
        return duel;
    }

    public async Task SubmitAnswerAsync(Guid answerId)
    {
        var userId = _context.Identity.Id;
        
        var duelInProgress = await _duelsRepository.GetCurrentDuelForPlayerAsync(userId);
        if (duelInProgress is null)
        {
            return;
        }

        var isAnswerValid = await _questionsModuleApi.CheckAnswerAsync(answerId);
        
        duelInProgress.SubmitAnswer(userId, isAnswerValid);
        
        if (duelInProgress.GetCurrentRound().IsCompleted())
        {
            if (duelInProgress.IsLastRound())
            {
                await _realTimeNotifier.NotifyMultipleUsersAsync(
                    [duelInProgress.PlayerOneId, duelInProgress.PlayerTwoId],
                    RealTimeNotificationEventTypes.DuelCompleted);
                
                duelInProgress.Complete();
            }
            else
            {
                var nextRound = duelInProgress.GetNextRound();
                var nextRoundDto = await BuildNextRoundDto(nextRound);
            
                await _realTimeNotifier.NotifyMultipleUsersAsync(
                    [duelInProgress.PlayerOneId, duelInProgress.PlayerTwoId],
                    RealTimeNotificationEventTypes.RoundCompleted,
                    nextRoundDto);
            }
        }
        
        await _duelsRepository.UpdateDuelAsync(duelInProgress);
    }
    
    public async Task<DuelRoundDto?> GetCurrentRoundAsync()
    {
        var userId = _context.Identity.Id;
        
        var duelInProgress = await _duelsRepository.GetCurrentDuelForPlayerAsync(userId);
        if (duelInProgress is null)
        {
            return null;
        }

        var currentQuestionId = duelInProgress.GetCurrentRound().QuestionId;
        var currentQuestionWithAnswers =  await _questionsModuleApi.GetQuestionWithAnswersByIdAsync(currentQuestionId);

        if (currentQuestionWithAnswers is null)
        {
            throw new QuestionNotFoundException(currentQuestionId);
        }
        
        return new DuelRoundDto(
            duelInProgress.CurrentRound,
            currentQuestionId,
            currentQuestionWithAnswers.Title,
            currentQuestionWithAnswers.Answers.Select(x => new AnswerDto(x.Id, x.Content)).ToList()
        );
    }

    public async Task<bool> AbandonDuelAsync()
    {
        var userId = _context.Identity.Id;
        
        var duelInProgress = await _duelsRepository.GetCurrentDuelForPlayerAsync(userId);
        if (duelInProgress is null)
        {
            _logger.LogWarning("No duel in progress found when trying to abandon for user with id: {userId}", userId);
            return false;
        }
        
        duelInProgress.Abandon(userId);
        await _duelsRepository.UpdateDuelAsync(duelInProgress);

        var duelParticipants = new List<Guid> { duelInProgress.PlayerOneId, duelInProgress.PlayerTwoId };
        await _realTimeNotifier.NotifyMultipleUsersAsync(duelParticipants, RealTimeNotificationEventTypes.DuelAbandoned);
        
        return true;
    }

    private async Task<bool> AnyPlayerCurrentlyInDuel(Guid playerOneId, Guid playerTwoId)
    {
        return await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerOneId)
            || await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerTwoId);
    }
    
    private async Task<DuelRoundDto> BuildNextRoundDto(DuelRound duelRound)
    {
        var questionWithAnswers = await _questionsModuleApi.GetQuestionWithAnswersByIdAsync(duelRound.QuestionId);
        return new DuelRoundDto(
            duelRound.Number,
            duelRound.QuestionId,
            questionWithAnswers!.Title,
            questionWithAnswers.Answers.Select(x => new AnswerDto
            (
                x.Id,
                x.Content
            )).ToList()
        );
    }
}