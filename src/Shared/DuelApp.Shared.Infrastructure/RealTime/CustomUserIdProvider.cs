using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace DuelApp.Shared.Infrastructure.RealTime;

public sealed class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var userId = connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           connection.User.FindFirst("sub")?.Value;
        return userId;
    }
}