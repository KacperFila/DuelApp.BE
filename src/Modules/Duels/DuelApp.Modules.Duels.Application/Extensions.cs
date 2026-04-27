using DuelApp.Modules.Duels.Application.Services;
using DuelApp.Modules.Duels.Application.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Duels.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDuelService, DuelService>();
        
        return services;
    }
}