using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Duels.Domain;

public static class Extensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services;
    }
}