using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Shared.Infrastructure.RealTime;

internal static class Extensions
{
    internal static IServiceCollection AddRealTime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        
        return services;
    }
}