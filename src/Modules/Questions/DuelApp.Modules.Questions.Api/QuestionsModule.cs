using DuelApp.Modules.Questions.Application;
using DuelApp.Modules.Questions.Infrastructure;
using DuelApp.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DuelApp.Modules.Questions.Api;

internal class QuestionsModule : IModule
{
    public const string BasePath = "questions-module";
    public string Name { get; } = "Questions";
    public string Path => BasePath;

    public IEnumerable<string> Policies { get; } =
    [
        "questions"
    ];

    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, hostEnvironment);
    }
        
    public void Use(IApplicationBuilder app)
    {
    }
}
