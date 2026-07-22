using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Exceptions;
using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Shared.Abstractions.Events;
using DuelApp.Shared.Abstractions.RealTime;

namespace DuelApp.Modules.Duels.Application.Events.External.Handlers;

public class MatchFoundEventHandler : IEventHandler<MatchFoundEvent>
{
    private readonly IDuelsService _duelsService;
    private readonly IRealTimeNotifier _realTimeNotifier;
    
    public MatchFoundEventHandler(
        IDuelsService duelsService,
        IRealTimeNotifier realTimeNotifier)
    {
        _duelsService = duelsService;
        _realTimeNotifier = realTimeNotifier;
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

        await _duelsService.StartDuelAsync(duelId.Value);
        
        await _realTimeNotifier.NotifyMultipleUsersAsync(
            [@event.PlayerOneId, @event.PlayerTwoId],
            RealTimeNotificationEventTypes.DuelStarted,
            new DuelStartedResponse(duelId.Value));
    }
}