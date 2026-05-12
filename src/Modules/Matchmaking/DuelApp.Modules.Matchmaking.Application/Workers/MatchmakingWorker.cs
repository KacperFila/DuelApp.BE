using DuelApp.Modules.Matchmaking.Application.Abstractions;
using DuelApp.Modules.Matchmaking.Application.Services;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Matchmaking.Application.Workers;

public sealed class MatchmakingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MatchmakingWorker> _logger;
    
    public MatchmakingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<MatchmakingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var matchmakingService = scope.ServiceProvider.GetRequiredService<IMatchmakingService>();
                
                await matchmakingService.TryMatchPlayersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Matchmaking worker failed");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}