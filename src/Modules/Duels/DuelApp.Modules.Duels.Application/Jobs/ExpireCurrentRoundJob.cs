using DuelApp.Modules.Duels.Application.Services;
using Quartz;

namespace DuelApp.Modules.Duels.Application.Jobs;

public sealed class ExpireCurrentRoundJob : IJob
{
    private readonly IDuelsService _duelsService;

    public ExpireCurrentRoundJob(IDuelsService duelsService)
    {
        _duelsService = duelsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var roundId = Guid.Parse(
            context.MergedJobDataMap.GetString("roundId")!);

        await _duelsService.ExpireCurrentRoundAsync(roundId);
    }
}
