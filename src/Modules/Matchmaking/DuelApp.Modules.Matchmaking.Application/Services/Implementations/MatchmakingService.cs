using DuelApp.Modules.Duels.Shared;
using DuelApp.Modules.Matchmaking.Application.Abstractions;
using DuelApp.Modules.Matchmaking.Application.Constants;
using DuelApp.Modules.Matchmaking.Application.Events;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;
using DuelApp.Shared.Abstractions.Messaging;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Matchmaking.Application.Services.Implementations;

public sealed class MatchmakingService : IMatchmakingService
{
    private readonly IMatchmakingRepository _matchmakingRepository;
    private readonly IMatchmakingUnitOfWork _unitOfWork;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<MatchmakingService> _logger;
    private readonly IDuelsModuleApi _duelsModuleApi;
    private readonly IRealTimeNotifier _realTimeNotifier;
    
    public MatchmakingService(
        IMatchmakingRepository matchmakingRepository,
        IMatchmakingUnitOfWork unitOfWork,
        IMessageBroker messageBroker,
        ILogger<MatchmakingService> logger,
        IDuelsModuleApi duelsModuleApi,
        IRealTimeNotifier realTimeNotifier)
    {
        _matchmakingRepository = matchmakingRepository;
        _unitOfWork = unitOfWork;
        _messageBroker = messageBroker;
        _logger = logger;
        _duelsModuleApi = duelsModuleApi;
        _realTimeNotifier = realTimeNotifier;
    }

    /// <summary>
    /// Attempts to enqueue a user into the matchmaking queue.
    /// The operation fails if the user is already queued or currently in a duel.
    /// </summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <returns>
    /// True if the user was enqueued successfully; otherwise false.
    /// </returns>
    public async Task<bool> TryJoinQueueAsync(Guid userId)
    {
        var isPlayerCurrentlyInQueue = await _matchmakingRepository.IsUserInQueueAsync(userId);
        if (isPlayerCurrentlyInQueue)
        {
            return false;
        }
        
        var isPlayerCurrentlyInDuel = await _duelsModuleApi.IsPlayerCurrentlyInDuelAsync(userId);
        if (isPlayerCurrentlyInDuel)
        {
            return false;
        }
        
        var entry = QueueEntry.Create(userId);

        try
        {
            await _matchmakingRepository.AddAsync(entry);
        }
        catch (DbUpdateException)
        {
            _logger.LogError("Failed to join matchmaking for user {UserId}", userId);
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Removes a player from the matchmaking queue by marking their entry as matched.
    /// If the player is not in the queue, the operation does nothing.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    public async Task LeaveQueueAsync(Guid playerId)
    {
        var entry = await _matchmakingRepository.GetByPlayerIdAsync(playerId);
        if (entry is null)
        {
            return;
        }
        
        entry.MarkAsCancelled();
        await _matchmakingRepository.UpdateAsync(entry);
    }
}