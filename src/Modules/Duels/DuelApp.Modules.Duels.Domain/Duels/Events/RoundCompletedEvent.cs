using DuelApp.Shared.Abstractions.Kernel;

namespace DuelApp.Modules.Duels.Domain.Duels.Events;

public record RoundCompletedEvent(
    Guid DuelId,
    int CompletedRoundNumber,
    Guid PlayerOneId,
    Guid PlayerTwoId,
    bool IsDuelCompleted,
    int? NextRoundNumber = null,
    int? TotalRounds = null,
    Guid? NextQuestionId = null,
    Guid? NextRoundId = null) : IDomainEvent;
