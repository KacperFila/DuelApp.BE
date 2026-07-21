using DuelApp.Shared.Abstractions.Kernel;

namespace DuelApp.Modules.Duels.Domain.Duels.Events;

public record RoundCompletedEvent(
    Guid DuelId,
    int RoundNumber,
    Guid PlayerOneId,
    Guid PlayerTwoId,
    bool IsDuelCompleted,
    int? NextRoundNumber = null,
    int? TotalRounds = null,
    Guid? NextQuestionId = null) : IDomainEvent;
