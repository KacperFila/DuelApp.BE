using DuelApp.Modules.Matchmaking.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace DuelApp.Modules.Matchmaking.Api.Hubs;

public sealed class MatchmakingHub : Hub
{
    private readonly IMatchmakingService _service;

    public MatchmakingHub(
        IMatchmakingService service)
    {
        _service = service;
    }

    public async Task TestUserRouting()
    {
        var userId = Context.UserIdentifier;

        await Clients.User(userId)
            .SendAsync("TestMessage", $"Hello user {userId}");
    }
}