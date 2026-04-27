using DuelApp.Modules.Duels.Domain.Duels.Repositories;
using DuelApp.Modules.Duels.Infrastructure.EF.Repositories;
using DuelApp.Modules.Duels.Infrastructure.Realtime;
using DuelApp.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Duels.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<DuelsDbContext>();

        services.AddScoped<IDuelRepository, DuelRepository>();
        services.AddSingleton<ConnectionManager>();
        services.AddSingleton<DuelSessionManager>();

        services.AddSignalR();
        
        return services;
    }
}