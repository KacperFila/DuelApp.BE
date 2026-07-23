using DuelApp.Shared.Abstractions.Kernel;

namespace DuelApp.Modules.Duels.Domain.Duels.Events;

public sealed record RoundStartedEvent(
    Guid DuelId,
    Guid RoundId,
    DateTime EndsAt
) : IDomainEvent;