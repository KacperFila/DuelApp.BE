using DuelApp.Modules.Matchmaking.Application.Abstractions;
using DuelApp.Modules.Matchmaking.Application.Constants;
using DuelApp.Modules.Matchmaking.Application.Events;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;
using DuelApp.Shared.Abstractions.Messaging;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Matchmaking.Application.Workers;

public sealed class MatchmakingWorker : BackgroundService
{
    private readonly ILogger<MatchmakingWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MatchmakingWorker(
        ILogger<MatchmakingWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var matchmakingRepository =
                    scope.ServiceProvider.GetRequiredService<IMatchmakingRepository>();

                var unitOfWork =
                    scope.ServiceProvider.GetRequiredService<IMatchmakingUnitOfWork>();

                var messageBroker =
                    scope.ServiceProvider.GetRequiredService<IMessageBroker>();

                var realTimeNotifier =
                    scope.ServiceProvider.GetRequiredService<IRealTimeNotifier>();

                await TryMatchPlayersAsync(
                    matchmakingRepository,
                    unitOfWork,
                    messageBroker,
                    realTimeNotifier,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Matchmaking worker failed");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task TryMatchPlayersAsync(
        IMatchmakingRepository matchmakingRepository,
        IMatchmakingUnitOfWork unitOfWork,
        IMessageBroker messageBroker,
        IRealTimeNotifier realTimeNotifier,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteAsync(async () =>
        {
            var enqueuedPlayers = await matchmakingRepository.GetQueuedBatchAsync(50);

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

                await MarkPairAsMatchedAsync(matchmakingRepository, pair);

                await messageBroker.PublishAsync(
                    new MatchFoundEvent(
                        pair[0].PlayerId,
                        pair[1].PlayerId));

                await realTimeNotifier.NotifyMultipleUsersAsync(
                    pair.Select(x => x.PlayerId),
                    RealTimeNotificationEventTypes.MatchFound);
            }
        }, cancellationToken);
    }

    private async Task MarkPairAsMatchedAsync(
        IMatchmakingRepository matchmakingRepository,
        IEnumerable<QueueEntry> players)
    {
        foreach (var player in players)
        {
            player.MarkAsMatched();
        }

        await matchmakingRepository.BulkUpdateAsync(players);
    }
}