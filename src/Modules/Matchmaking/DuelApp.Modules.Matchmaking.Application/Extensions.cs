using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Matchmaking.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}