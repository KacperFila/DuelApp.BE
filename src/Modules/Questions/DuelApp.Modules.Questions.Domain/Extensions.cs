using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Questions.Domain;

public static class Extensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services;
    }
}