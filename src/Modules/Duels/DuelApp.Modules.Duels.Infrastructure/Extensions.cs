using DuelApp.Modules.Duels.Application.Abstractions;
using DuelApp.Modules.Duels.Infrastructure.EF.Repositories;
using DuelApp.Modules.Duels.Infrastructure.Services;
using DuelApp.Modules.Duels.Shared;
using DuelApp.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Duels.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<DuelsDbContext>();
        services.AddScoped<IDuelsRepository, DuelsRepository>();
        services.AddScoped<IDuelsModuleApi, DuelsModuleApi>();
        
        return services;
    }
}