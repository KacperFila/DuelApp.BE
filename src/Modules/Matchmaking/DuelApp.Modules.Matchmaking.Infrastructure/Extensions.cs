using DuelApp.Modules.Matchmaking.Application.Abstractions;
using DuelApp.Modules.Matchmaking.Infrastructure.EF.Repositories;
using Microsoft.Extensions.DependencyInjection;
using DuelApp.Shared.Infrastructure.Postgres;

namespace DuelApp.Modules.Matchmaking.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<MatchmakingDbContext>();
        services.AddScoped<IMatchmakingRepository, MatchmakingRepository>();
        
        return services;
    }
}