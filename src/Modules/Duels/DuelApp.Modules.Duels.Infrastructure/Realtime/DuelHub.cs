using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Modules.Matchmaking.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DuelApp.Modules.Duels.Infrastructure.Realtime;

public class DuelHub : Hub
{
    private readonly ConnectionManager connectionManager;
    private readonly DuelSessionManager sessionManager;
    private readonly IDuelService duelService;
    private readonly IMatchmakingModuleApi matchmakingModuleApi;
    private readonly ILogger<DuelHub> logger;

    public DuelHub(
        ConnectionManager connectionManager,
        DuelSessionManager sessionManager,
        IDuelService duelService,
        IMatchmakingModuleApi matchmakingModuleApi, ILogger<DuelHub> logger)
    {
        this.connectionManager = connectionManager;
        this.sessionManager = sessionManager;
        this.duelService = duelService;
        this.matchmakingModuleApi = matchmakingModuleApi;
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userId = Guid.Parse(httpContext.Request.Query["userId"]);
        
        // var userId = Guid.Parse(Context.UserIdentifier!);
        connectionManager.Add(userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = connectionManager.GetUserId(Context.ConnectionId);
        if (userId.HasValue)
        {
            connectionManager.Remove(userId.Value);

            var duelId = sessionManager.GetDuelId(userId.Value);
            if (duelId.HasValue)
            {
                await Clients.Group(duelId.Value.ToString())
                    .SendAsync("OpponentDisconnected");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinQueue()
    {
        var httpContext = Context.GetHttpContext();
        var userId = Guid.Parse(httpContext.Request.Query["userId"]);
        // var userId = Guid.Parse(Context.UserIdentifier!);

        var match = matchmakingModuleApi.JoinQueue(userId);
        if (match == null)
            return;

        var duelId = await duelService.Create(match.Player1, match.Player2, 5);
        logger.LogInformation("Duel between user with Id: {Player1Id} and {Player2Id} has been created", match.Player1, match.Player2);

        foreach (var player in new[] { match.Player1, match.Player2 })
        {
            var connId = connectionManager.GetConnectionId(player);
            if (connId != null)
            {
                await Groups.AddToGroupAsync(connId, duelId.ToString());
            }
        }

        await Clients.Group(duelId.ToString())
            .SendAsync("GameStarted");
    }

    public async Task SubmitAnswer(bool isCorrect)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);
        var duelId = sessionManager.GetDuelId(userId).Value;

        var session = sessionManager.GetSession(duelId);
        
        // Now DuelService is pure, Hub handles runtime locking
        var state = duelService.SubmitAnswer(duelId, userId, isCorrect).Result;

        Clients.Group(duelId.ToString()).SendAsync("GameState", state);

        if (state.Status == "Completed")
        {
            Clients.Group(duelId.ToString()).SendAsync("GameEnded", state);
        }
    }
}