using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.RealTime;

public interface IRealTimeNotifier
{
    Task NotifyUserAsync(Guid userId, string eventName, object payload);
    Task NotifyUserAsync(Guid userId, string eventName);
    Task NotifyMultipleUsersAsync(IEnumerable<Guid> userIds, string eventName, object payload);
    Task NotifyMultipleUsersAsync(IEnumerable<Guid> userIds, string eventName);
}