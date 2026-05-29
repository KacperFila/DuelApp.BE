using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Shared.Infrastructure.Contexts;

public static class Extensions
{
    
    public static IServiceCollection AddContextExtensions(this IServiceCollection services)
    {
        services.AddScoped<IContextFactory, ContextFactory>();
        services.AddSingleton<IContextAccessor, ContextAccessor>();
        services.AddScoped<ContextMiddleware>();
        services.AddScoped<IContext>(sp =>
            sp.GetRequiredService<IContextAccessor>().Current);
        
        return services;
    }
}