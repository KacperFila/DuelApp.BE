using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
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
        
        var duel = Duel.Create(playerOneId, playerTwoId, RoundsCount);
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

    public async Task<bool> CreateNextRoundAsync(Guid duelId, Guid questionId)
    {
        var duel = await _duelsRepository.GetByIdAsync(duelId);
        if (duel is null)
        {
            return false;
        }

        var questionWithAnswers = await _questionsModuleApi.GetQuestionsWithAnswersAsync(questionsAmount: 1);
        if (!questionWithAnswers.Any())
        {
            return false;
        }
        
        duel.CreateNextRound(questionWithAnswers.First().Id);
        await _duelsRepository.UpdateDuelAsync(duel);
        
        return true;
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
}