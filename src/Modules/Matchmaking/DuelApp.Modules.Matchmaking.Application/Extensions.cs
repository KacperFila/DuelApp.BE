using DuelApp.Modules.Matchmaking.Application.Services;
using DuelApp.Modules.Matchmaking.Application.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Matchmaking.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMatchmakingManager, MatchmakingManager>();
        
        return services;
    }
}