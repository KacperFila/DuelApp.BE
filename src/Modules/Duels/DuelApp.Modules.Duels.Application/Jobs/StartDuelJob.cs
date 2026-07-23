using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Shared.Abstractions.RealTime;
using Quartz;

namespace DuelApp.Modules.Duels.Application.Jobs;

public sealed class StartDuelJob : IJob
{
    private readonly IDuelsService _duelsService;
    private readonly IRealTimeNotifier _realTimeNotifier;
    
    public StartDuelJob(IDuelsService duelsService, IRealTimeNotifier realTimeNotifier)
    {
        _duelsService = duelsService;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var duelId = Guid.Parse(
            context.MergedJobDataMap.GetString("duelId")!);
        var playerOneId = Guid.Parse(
            context.MergedJobDataMap.GetString("playerOneId")!);
        var playerTwoId = Guid.Parse(
            context.MergedJobDataMap.GetString("playerTwoId")!);

        await _duelsService.StartDuelAsync(duelId);
        
        await _realTimeNotifier.NotifyMultipleUsersAsync(
            [playerOneId, playerTwoId],
            RealTimeNotificationEventTypes.DuelStarted,
            new DuelStartedResponse(duelId));
    }
}