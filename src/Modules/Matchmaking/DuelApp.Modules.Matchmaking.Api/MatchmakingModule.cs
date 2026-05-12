using DuelApp.Modules.Matchmaking.Application;
using DuelApp.Modules.Matchmaking.Infrastructure;
using DuelApp.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Matchmaking.Api;

internal class MatchmakingModule : IModule
{
    public const string BasePath = "matchmaking-module";
    public string Name { get; } = "Matchmaking";
    public string Path => BasePath;

    public IEnumerable<string> Policies { get; } =
    [
        "matchmaking"
    ];

    public void Register(IServiceCollection services)
    {
        services.AddApplication();
        services.AddApplication();
        services.AddInfrastructure();
    }
        
    public void Use(IApplicationBuilder app)
    {
    }
}