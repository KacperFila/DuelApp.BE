using DuelApp.Modules.Matchmaking.Application.Services;
using DuelApp.Modules.Matchmaking.Application.Services.Implementations;
using DuelApp.Modules.Matchmaking.Application.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Matchmaking.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMatchmakingService, MatchmakingService>();
        services.AddHostedService<MatchmakingWorker>();
        
        return services;
    }
}