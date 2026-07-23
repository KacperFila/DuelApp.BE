using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Application.Configuration;
using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Exceptions;
using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using DuelApp.Modules.Duels.Domain.Duels.ValueObjects;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Modules.Users.Shared;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DuelApp.Modules.Duels.Application.Services;

public class DuelsService : IDuelsService
{
    private readonly IDuelsRepository _duelsRepository;
    private readonly ILogger<DuelsService> _logger;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IQuestionsModuleApi _questionsModuleApi;
    private readonly IUsersModuleApi _usersModuleApi;
    private readonly IDuelsUnitOfWork _unitOfWork;
    private readonly DuelConfiguration _duelConfiguration;

    public DuelsService(
        IDuelsRepository duelsRepository,
        ILogger<DuelsService> logger,
        IRealTimeNotifier realTimeNotifier,
        IQuestionsModuleApi questionsModuleApi,
        IUsersModuleApi usersModuleApi,
        IDuelsUnitOfWork unitOfWork,
        IOptionsSnapshot<DuelConfiguration> duelConfiguration)
    {
        _duelsRepository = duelsRepository;
        _logger = logger;
        _realTimeNotifier = realTimeNotifier;
        _questionsModuleApi = questionsModuleApi;
        _usersModuleApi = usersModuleApi;
        _unitOfWork = unitOfWork;
        _duelConfiguration = duelConfiguration.Value;
    }

    public async Task<Guid?> CreateDuelAsync(Guid playerOneId, Guid playerTwoId)
    {
        var createdDuelId = (Guid?)null;
        
        await _unitOfWork.ExecuteAsync(async () =>
        {
            if (await AnyPlayerCurrentlyInDuel(playerOneId, playerTwoId))
            {
                _logger.LogWarning(
                    "New duel cannot be created for players: {Player1} and {Player2}- at least one of the players is currently in duel.",
                    playerOneId,
                    playerTwoId);
            }

            var questions = await _questionsModuleApi.GetQuestionsWithAnswersAsync(_duelConfiguration.DuelRoundCount);

            var rounds = new List<DuelRound>();
            foreach (var (question, index) in questions.Select((value, index) => (value, index)))
            {
                rounds.Add(DuelRound.Create(index + 1, question.Id));
            }

            var duel = Duel.Create(
                playerOneId,
                playerTwoId,
                rounds,
                TimeSpan.FromSeconds(_duelConfiguration.RoundDurationSeconds));
            
            await _duelsRepository.CreateDuelAsync(duel);

            createdDuelId = duel.Id;
        });

        return createdDuelId;
    }

    public async Task StartDuelAsync(Guid duelId)
    {
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var duel = await _duelsRepository.GetByIdAsync(duelId);
            if (duel is null)
            {
                throw new DuelNotFoundException(duelId);
            }

            duel.Start();

            await _duelsRepository.UpdateDuelAsync(duel);
        });
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

    public async Task SubmitAnswerForUserAsync(Guid answerId, Guid roundId, Guid userId)
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

            var currentRound = duelInProgress.GetCurrentRound();
            if (currentRound.Id != roundId)
            {
                return;
            }
            
            var question = await _questionsModuleApi.GetQuestionWithAnswersByIdAsync(currentRound.QuestionId);
            
            var answer = question?.Answers.SingleOrDefault(x => x.Id == answerId);
            if (answer is null)
            {
                return;
            }

            duelInProgress.SubmitAnswer(userId, roundId, answer.IsCorrect);
            
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

        var currentRound = duelInProgress.GetCurrentRound();
        
        return new DuelRoundDto(
            currentRound.Id,
            duelInProgress.CurrentRound,
            duelInProgress.TotalRounds,
            currentQuestionId,
            currentQuestionWithAnswers.Title,
            currentQuestionWithAnswers.Answers.Select(x => new AnswerDto(x.Id, x.Content)).ToList(),
            currentRound.EndsAt!.Value,
            duelInProgress.RoundDuration.Seconds
        );
    }

    public async Task AbandonDuelForUserAsync(Guid userId)
    {
        await _unitOfWork.ExecuteAsync(async () =>
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
            await _realTimeNotifier.NotifyMultipleUsersAsync(duelParticipants,
                RealTimeNotificationEventTypes.DuelAbandoned);
        });
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

    public async Task ExpireCurrentRoundAsync(Guid roundId)
    {
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var duel = await _duelsRepository.GetForUpdateByRoundIdAsync(roundId);
            if (duel is null)
            {
                return;
            }
            
            duel.ExpireRound(roundId);
            
            await _duelsRepository.UpdateDuelAsync(duel);
        });
    }

    private async Task<bool> AnyPlayerCurrentlyInDuel(Guid playerOneId, Guid playerTwoId)
    {
        return await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerOneId)
            || await _duelsRepository.IsPlayerCurrentlyInDuelAsync(playerTwoId);
    }
}
