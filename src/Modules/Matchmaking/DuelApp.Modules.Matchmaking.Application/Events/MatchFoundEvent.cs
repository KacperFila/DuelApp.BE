using DuelApp.Shared.Abstractions.Events;

namespace DuelApp.Modules.Matchmaking.Application.Events;

public record MatchFoundEvent(Guid PlayerOneId, Guid PlayerTwoId) : IEvent;