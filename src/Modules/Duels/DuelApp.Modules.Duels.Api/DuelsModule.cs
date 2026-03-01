using DuelApp.Modules.Duels.Application;
using DuelApp.Modules.Duels.Infrastructure;
using DuelApp.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Duels.Api;

internal class DuelsModule : IModule
{
    public const string BasePath = "duels-module";
    public string Name { get; } = "Duels";
    public string Path => BasePath;

    public IEnumerable<string> Policies { get; } =
    [
        "duels"
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