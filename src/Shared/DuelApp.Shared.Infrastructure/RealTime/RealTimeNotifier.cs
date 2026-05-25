using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using IRealTimeNotifier = DuelApp.Shared.Abstractions.RealTime.IRealTimeNotifier;

namespace DuelApp.Shared.Infrastructure.RealTime;

public class RealTimeNotifier : IRealTimeNotifier
{
    private readonly IHubContext<GameHub> _hub;

    public RealTimeNotifier(IHubContext<GameHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyUserAsync(Guid userId, string eventName, object payload)
    {
        return _hub.Clients.User(userId.ToString())
            .SendAsync(eventName, payload);
    }

    public Task NotifyUserAsync(Guid userId, string eventName)
    {
        return _hub.Clients.User(userId.ToString())
            .SendAsync(eventName);
    }

    public Task NotifyMultipleUsersAsync(IEnumerable<Guid> userIds, string eventName, object payload)
    {
        var tasks = userIds
            .Select(userId => _hub.Clients.User(userId.ToString())
            .SendAsync(eventName, payload));

        return Task.WhenAll(tasks);
    }

    public Task NotifyMultipleUsersAsync(IEnumerable<Guid> userIds, string eventName)
    {
        var tasks = userIds
            .Select(userId => _hub.Clients.User(userId.ToString())
            .SendAsync(eventName));

        return Task.WhenAll(tasks);
    }
}