using DuelApp.Modules.Duels.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Duels.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDuelsService, DuelsService>();
        
        return services;
    }
}