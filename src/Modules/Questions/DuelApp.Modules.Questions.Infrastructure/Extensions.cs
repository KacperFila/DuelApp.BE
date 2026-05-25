using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Infrastructure.EF.Repositories;
using DuelApp.Modules.Questions.Infrastructure.Services;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Questions.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<QuestionsDbContext>();
        services.AddScoped<IAnswersRepository, AnswersRepository>();
        services.AddScoped<IQuestionsRepository, QuestionsRepository>();
        services.AddScoped<IQuestionsModuleApi, QuestionsModuleApi>();
        
        return services;
    }
}