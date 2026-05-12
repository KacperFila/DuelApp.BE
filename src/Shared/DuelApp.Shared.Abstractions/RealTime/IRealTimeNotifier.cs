using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.RealTime;

public interface IRealTimeNotifier
{
    Task SendToUserAsync(Guid userId, string eventName, object payload);
    Task SendToMultipleUsersAsync(IEnumerable<Guid> userIds, string eventName, object payload);
}