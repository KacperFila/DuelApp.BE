using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Exceptions;
using DuelApp.Modules.Duels.Application.Models;
using DuelApp.Modules.Duels.Domain.Duels.Events;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Shared.Abstractions.Kernel;
using DuelApp.Shared.Abstractions.RealTime;

namespace DuelApp.Modules.Duels.Application.Events.Domain.Handlers;

public sealed class RoundCompletedEventHandler : IDomainEventHandler<RoundCompletedEvent>
{
    private readonly IQuestionsModuleApi _questionsModuleApi;
    private readonly IRealTimeNotifier _realTimeNotifier;

    public RoundCompletedEventHandler(
        IQuestionsModuleApi questionsModuleApi,
        IRealTimeNotifier realTimeNotifier)
    {
        _questionsModuleApi = questionsModuleApi;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task HandleAsync(RoundCompletedEvent @event)
    {
        var participants = new[] { @event.PlayerOneId, @event.PlayerTwoId };

        if (@event.IsDuelCompleted)
        {
            await _realTimeNotifier.NotifyMultipleUsersAsync(
                participants,
                RealTimeNotificationEventTypes.DuelCompleted);
            
            return;
        }

        if (!IsNextRoundDataValid(@event))
        {
            throw new NoRoundDetailsFoundException(@event.DuelId, @event.CompletedRoundNumber);
        }

        var question = await _questionsModuleApi.GetQuestionWithAnswersByIdAsync(@event.NextQuestionId!.Value)
            ?? throw new QuestionNotFoundException(@event.NextQuestionId.Value);

        var nextRound = new DuelRoundDto(
            @event.NextRoundId!.Value,
            @event.NextRoundNumber!.Value,
            @event.TotalRounds!.Value,
            @event.NextQuestionId.Value,
            question.Title,
            question.Answers.Select(x => new AnswerDto(x.Id, x.Content)).ToList());

        await _realTimeNotifier.NotifyMultipleUsersAsync(
            participants,
            RealTimeNotificationEventTypes.RoundCompleted,
            nextRound);
    }

    private bool IsNextRoundDataValid(RoundCompletedEvent @event)
    {
        return @event.NextRoundNumber is not null &&
               @event.TotalRounds is not null &&
               @event.NextQuestionId is not null &&
               @event.NextRoundId is not null;
    }
}
