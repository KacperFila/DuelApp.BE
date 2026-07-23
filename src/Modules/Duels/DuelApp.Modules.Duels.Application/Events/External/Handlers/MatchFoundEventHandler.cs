using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Jobs;
using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Shared.Abstractions.Events;
using DuelApp.Shared.Abstractions.RealTime;
using Quartz;

namespace DuelApp.Modules.Duels.Application.Events.External.Handlers;

public class MatchFoundEventHandler : IEventHandler<MatchFoundEvent>
{
    private readonly IDuelsService _duelsService;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly ISchedulerFactory _schedulerFactory;

    private const int DuelPreviewSeconds = 1;
    
    public MatchFoundEventHandler(
        IDuelsService duelsService,
        IRealTimeNotifier realTimeNotifier,
        ISchedulerFactory schedulerFactory)
    {
        _duelsService = duelsService;
        _realTimeNotifier = realTimeNotifier;
        _schedulerFactory = schedulerFactory;

    }

    public async Task HandleAsync(MatchFoundEvent @event)
    {
        var duelId = await _duelsService.CreateDuelAsync(
            @event.PlayerOneId,
            @event.PlayerTwoId);

        if (duelId is null)
        {
            return;
        }
        
        var scheduler = await _schedulerFactory.GetScheduler();

        var jobKey = new JobKey(
            $"start-duel-{duelId}");

        var triggerKey = new TriggerKey(
            $"start-duel-trigger-{duelId}");
        
        var job = JobBuilder
            .Create<StartDuelJob>()
            .WithIdentity(jobKey)
            .UsingJobData("duelId", duelId.ToString())
            .UsingJobData("playerOneId", @event.PlayerOneId.ToString())
            .UsingJobData("playerTwoId", @event.PlayerTwoId.ToString())
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity(triggerKey)
            .StartAt(DateTime.UtcNow.AddSeconds(DuelPreviewSeconds))
            .Build();
  
        await scheduler.ScheduleJob(job, trigger);
        
        await _realTimeNotifier.NotifyMultipleUsersAsync(
            [@event.PlayerOneId, @event.PlayerTwoId],
            RealTimeNotificationEventTypes.OpponentFound,
            new OpponentFoundDto());
    }
}