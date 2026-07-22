using DuelApp.Modules.Duels.Application.Configuration;
using DuelApp.Modules.Duels.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DuelApp.Modules.Duels.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDuelsService, DuelsService>();
        services.AddOptions<DuelConfiguration>()
            .Bind(configuration.GetSection(DuelConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
        });
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        
        return services;
    }
}