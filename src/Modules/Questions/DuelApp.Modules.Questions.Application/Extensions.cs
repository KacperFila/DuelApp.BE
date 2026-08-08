using DuelApp.Modules.Questions.Application.Services;
using DuelApp.Modules.Questions.Application.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace DuelApp.Modules.Questions.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQuestionsService, QuestionsService>();
        services.AddSingleton<QuestionImportJsonReader>();
        services.AddSingleton<QuestionImportValidator>();
        services.AddScoped<QuestionImportService>();
        services.AddScoped<QuestionPublicationService>();

        return services;
    }
}
