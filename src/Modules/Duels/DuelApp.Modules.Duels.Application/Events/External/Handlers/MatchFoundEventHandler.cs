using DuelApp.Modules.Duels.Application.Constants;
using DuelApp.Modules.Duels.Application.Exceptions;
using DuelApp.Modules.Duels.Application.Responses;
using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Shared.Abstractions.Events;
using DuelApp.Shared.Abstractions.RealTime;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Duels.Application.Events.External.Handlers;

public class MatchFoundEventHandler : IEventHandler<MatchFoundEvent>
{
    private readonly IDuelsService _duelsService;
    private readonly IQuestionsModuleApi _questionsModuleApi;
    private readonly ILogger<MatchFoundEventHandler> _logger;
    private readonly IRealTimeNotifier _realTimeNotifier;
    
    public MatchFoundEventHandler(
        IDuelsService duelsService,
        ILogger<MatchFoundEventHandler> logger,
        IRealTimeNotifier realTimeNotifier,
        IQuestionsModuleApi questionsModuleApi)
    {
        _duelsService = duelsService;
        _logger = logger;
        _realTimeNotifier = realTimeNotifier;
        _questionsModuleApi = questionsModuleApi;
    }

    public async Task HandleAsync(MatchFoundEvent @event)
    {
        var duelId = await _duelsService.CreateDuelAsync(@event.PlayerOneId, @event.PlayerTwoId);
        if (duelId is null)
        {
            return;
        }
        
        var duel = await _duelsService.GetDuelByIdAsync(duelId.Value);
        if (duel is null)
        {
            throw new DuelNotFoundException(duelId.Value);
        }
        
        var questionWithAnswers = await _questionsModuleApi.GetQuestionsWithAnswersAsync(questionsAmount: 1);
        if (!questionWithAnswers.Any())
        {
            throw new NoQuestionsFoundException();
        }
        
        await _duelsService.CreateNextRoundAsync(duelId.Value, questionWithAnswers.First().Id);

        await _realTimeNotifier.NotifyMultipleUsersAsync(
            [@event.PlayerOneId, @event.PlayerTwoId],
            RealTimeNotificationEventTypes.DuelStarted,
            new DuelStartedResponse(duelId.Value));
    }
}