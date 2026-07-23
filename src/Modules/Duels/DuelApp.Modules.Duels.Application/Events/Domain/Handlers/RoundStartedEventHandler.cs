using DuelApp.Modules.Duels.Application.Jobs;
using Quartz;
using DuelApp.Modules.Duels.Domain.Duels.Events;
using DuelApp.Shared.Abstractions.Kernel;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Duels.Application.Events.Domain.Handlers;

public sealed class RoundStartedEventHandler 
    : IDomainEventHandler<RoundStartedEvent>
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<RoundStartedEventHandler> _logger;

    public RoundStartedEventHandler(
        ISchedulerFactory schedulerFactory,
        ILogger<RoundStartedEventHandler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task HandleAsync(RoundStartedEvent @event)
    {
        var scheduler = await _schedulerFactory.GetScheduler();

        var jobKey = new JobKey(
            $"end-round-{@event.RoundId}");

        var triggerKey = new TriggerKey(
            $"end-round-trigger-{@event.RoundId}");

        if (await scheduler.CheckExists(jobKey))
        {
            _logger.LogWarning(
                "Round expiration job already exists. JobKey: {JobKey}, RoundId: {RoundId}",
                jobKey,
                @event.RoundId);

            return;
        }
        _logger.LogInformation(
            "Round expiration scheduled for RoundId: {RoundId} at {time}, utc now: {utcnow}",
            @event.RoundId,
            @event.EndsAt,
            DateTime.UtcNow
            );
        
        var job = JobBuilder
            .Create<ExpireCurrentRoundJob>()
            .WithIdentity(jobKey)
            .UsingJobData(
                "roundId",
                @event.RoundId.ToString())
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity(triggerKey)
            .StartAt(@event.EndsAt)
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}