using DuelApp.Modules.Matchmaking.Infrastructure.Services;
using DuelApp.Modules.Matchmaking.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Matchmaking.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMatchmakingModuleApi, MatchmakingModuleApi>();
        
        return services;
    }
}