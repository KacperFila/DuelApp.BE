using DuelApp.Modules.Duels.Shared;
using DuelApp.Modules.Matchmaking.Application.Abstractions;
using DuelApp.Modules.Matchmaking.Application.Constants;
using DuelApp.Modules.Matchmaking.Application.Events;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;
using DuelApp.Shared.Abstractions.Contexts;
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
    private readonly IContext _context;
    
    public MatchmakingService(
        IMatchmakingRepository matchmakingRepository,
        IMatchmakingUnitOfWork unitOfWork,
        IMessageBroker messageBroker,
        ILogger<MatchmakingService> logger,
        IDuelsModuleApi duelsModuleApi,
        IRealTimeNotifier realTimeNotifier,
        IContext context)
    {
        _matchmakingRepository = matchmakingRepository;
        _unitOfWork = unitOfWork;
        _messageBroker = messageBroker;
        _logger = logger;
        _duelsModuleApi = duelsModuleApi;
        _realTimeNotifier = realTimeNotifier;
        _context = context;
    }

    /// <summary>
    /// Attempts to add a player to the matchmaking queue.
    /// Returns false if the player is already queued or if a concurrency conflict occurs.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>
    /// True if the player was successfully added to the queue; otherwise, false.
    /// </returns>
    public async Task<bool> TryJoinQueueAsync()
    {
        var userId = _context.Identity.Id;
        
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
        
        entry.MarkAsMatched();
        await _matchmakingRepository.UpdateAsync(entry);
    }

    /// <summary>
    /// Attempts to match players from the queue in pairs and publishes match events.
    /// Processes players in batches and pairs them sequentially.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task TryMatchPlayersAsync(CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteAsync(async () =>
        {            
            var enqueuedPlayers = await _matchmakingRepository.GetQueuedBatchAsync(50);
            if (enqueuedPlayers.Count < 2)
            {
                return;
            }
            
            foreach (var pair in enqueuedPlayers.Chunk(2))
            {
                if (pair.Length != 2)
                {
                    break;
                }
                
                await MarkPairAsMatchedAsync(pair);
                
                await _messageBroker.PublishAsync(
                    new MatchFoundEvent(
                        pair[0].PlayerId,
                        pair[1].PlayerId)
                );

                await _realTimeNotifier.NotifyMultipleUsersAsync(
                    pair.Select(x => x.PlayerId),
                    RealTimeNotificationEventTypes.MatchFound);
            }
        }, cancellationToken);
    }

    private async Task MarkPairAsMatchedAsync(IEnumerable<QueueEntry> players)
    {
        foreach (var player in players)
        {
            player.MarkAsMatched();
        }

        await _matchmakingRepository.BulkUpdateAsync(players);
    }
}