using DuelApp.Modules.Duels.Application.Responses;
using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Shared.Abstractions.Events;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Duels.Application.Events.External.Handlers;

public class MatchFoundEventHandler : IEventHandler<MatchFoundEvent>
{
    private readonly IDuelsService _duelsService;
    private readonly ILogger<MatchFoundEventHandler> _logger;
    private readonly IRealTimeNotifier _realTimeNotifier;
    
    public MatchFoundEventHandler(
        IDuelsService duelsService,
        ILogger<MatchFoundEventHandler> logger,
        IRealTimeNotifier realTimeNotifier)
    {
        _duelsService = duelsService;
        _logger = logger;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task HandleAsync(MatchFoundEvent @event)
    {
        var duelId = await _duelsService.CreateDuelAsync(@event.PlayerOneId, @event.PlayerTwoId);
        if (duelId is null)
        {
            return;
        }

        await _realTimeNotifier.SendToMultipleUsersAsync([@event.PlayerOneId, @event.PlayerTwoId], "DuelStarted", new DuelStartedResponse(duelId.Value));
    }
}