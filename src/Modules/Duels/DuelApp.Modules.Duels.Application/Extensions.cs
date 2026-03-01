using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Duels.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}