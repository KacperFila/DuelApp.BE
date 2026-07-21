using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Exceptions;
using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using DuelApp.Modules.Duels.Domain.Duels.ValueObjects;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Modules.Users.Shared;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Duels.Application.Services;

public class DuelsService : IDuelsService
{
    private readonly IDuelsRepository _duelsRepository;
    private readonly ILogger<DuelsService> _logger;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IQuestionsModuleApi _questionsModuleApi;
    private readonly IUsersModuleApi _usersModuleApi;
    private readonly IDuelsUnitOfWork _unitOfWork;
    private const int RoundsCount = 5;

    public DuelsService(
        IDuelsRepository duelsRepository,
        ILogger<DuelsService> logger,
        IRealTimeNotifier realTimeNotifier,
        IQuestionsModuleApi questionsModuleApi,
        IUsersModuleApi usersModuleApi,
        IDuelsUnitOfWork unitOfWork)
    {
        _duelsRepository = duelsRepository;
        _logger = logger;
        _realTimeNotifier = realTimeNotifier;
        _questionsModuleApi = questionsModuleApi;
        _usersModuleApi = usersModuleApi;
        _unitOfWork = unitOfWork;
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

    public async Task SubmitAnswerForUserAsync(Guid? answerId, Guid userId)
    {
        var currentDuelId = await _duelsRepository.GetCurrentDuelIdForPlayerAsync(userId);
        if (currentDuelId is null)
        {
            return;
        }
        
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var duelInProgress = await _duelsRepository.GetForUpdateById(currentDuelId.Value);
            if (duelInProgress is null)
            {
                return;
            }

            var isAnswerValid = answerId is not null && await _questionsModuleApi.CheckAnswerAsync(answerId.Value);

            duelInProgress.SubmitAnswer(userId, isAnswerValid);
            await _duelsRepository.UpdateDuelAsync(duelInProgress);
        });
    }
    
    public async Task<DuelRoundDto?> GetCurrentRoundForUserAsync(Guid userId)
    {
        var currentDuelId = await _duelsRepository.GetCurrentDuelIdForPlayerAsync(userId);
        if (currentDuelId is null)
        {
            return null;
        }
        
        var duelInProgress = await _duelsRepository.GetByIdAsync(currentDuelId.Value);
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
            duelInProgress.TotalRounds,
            currentQuestionId,
            currentQuestionWithAnswers.Title,
            currentQuestionWithAnswers.Answers.Select(x => new AnswerDto(x.Id, x.Content)).ToList()
        );
    }

    public async Task AbandonDuelForUserAsync(Guid userId)
    {
        var currentDuelId = await _duelsRepository.GetCurrentDuelIdForPlayerAsync(userId);
        if (currentDuelId is null)
        {
            return;
        }
        
        var duelInProgress = await _duelsRepository.GetByIdAsync(currentDuelId.Value);
        if (duelInProgress is null)
        {
            return;
        }
        
        duelInProgress.Abandon(userId);
        await _duelsRepository.UpdateDuelAsync(duelInProgress);

        var duelParticipants = new List<Guid> { duelInProgress.PlayerOneId, duelInProgress.PlayerTwoId };
        await _realTimeNotifier.NotifyMultipleUsersAsync(duelParticipants, RealTimeNotificationEventTypes.DuelAbandoned);
    }

    public async Task<DuelPreview?> GetCurrentDuelPreviewAsync(Guid userId)
    {
        var currentDuelId = await _duelsRepository.GetCurrentDuelIdForPlayerAsync(userId);
        if (currentDuelId is null)
        {
            return null;
        }
        
        var duelInProgress = await _duelsRepository.GetByIdAsync(currentDuelId.Value);
        if (duelInProgress is null)
        {
            return null;
        }
        
        var player = await _usersModuleApi.GetByKeycloakIdAsync(userId.ToString());
        var opponentId = duelInProgress.PlayerOneId == userId 
            ?  duelInProgress.PlayerTwoId
            : duelInProgress.PlayerOneId;
        var opponent = await _usersModuleApi.GetByKeycloakIdAsync(opponentId.ToString());

        if (player is null || opponent is null)
        {
            _logger.LogWarning("No pair of players found for duel with Id: {duelId}", duelInProgress);
            return null;
        }

        var result = new DuelPreview
        {
            Player = new DuelPlayer
            {
                Email = player.Email,
                TotalPoints = 0,
                AvatarUri = player.AvatarUri
            },
            Opponent = new DuelPlayer
            {
                Email = opponent.Email,
                TotalPoints = 0,
                AvatarUri = opponent.AvatarUri
            }
        };

        return result;
    }

    private async Task<bool> AnyPlayerCurrentlyInDuel(Guid playerOneId, Guid playerTwoId)
    {
        return await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerOneId)
            || await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerTwoId);
    }
    
}
