using DuelApp.Shared.Abstractions.Events;

namespace DuelApp.Modules.Duels.Application.Events.External;

public record MatchFoundEvent(Guid PlayerOneId, Guid PlayerTwoId) : IEvent;