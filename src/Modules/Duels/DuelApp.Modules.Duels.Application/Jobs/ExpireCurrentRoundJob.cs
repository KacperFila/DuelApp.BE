using DuelApp.Modules.Duels.Application.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DuelApp.Modules.Duels.Application.Jobs;

public sealed class ExpireCurrentRoundJob : IJob
{
    private readonly ILogger<ExpireCurrentRoundJob> _logger;
    private readonly IDuelsService _duelsService;

    public ExpireCurrentRoundJob(
        ILogger<ExpireCurrentRoundJob> logger,
        IDuelsService duelsService)
    {
        _logger = logger;
        _duelsService = duelsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var roundId = Guid.Parse(
            context.MergedJobDataMap.GetString("roundId")!);

        // _logger.LogInformation(
        //     "Executing round expiration job. RoundId: {RoundId}, FireTime: {FireTime}",
        //     roundId,
        //     context.FireTimeUtc);

        await _duelsService.ExpireCurrentRoundAsync(roundId);

        _logger.LogInformation(
            "Round expiration completed. RoundId: {RoundId}",
            roundId);
    }
}