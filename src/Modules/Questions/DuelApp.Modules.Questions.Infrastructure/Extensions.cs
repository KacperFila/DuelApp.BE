using Azure.Identity;
using Azure.Storage.Blobs;
using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Infrastructure.Const;
using DuelApp.Modules.Questions.Infrastructure.EF.Repositories;
using DuelApp.Modules.Questions.Infrastructure.Services;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DuelApp.Modules.Questions.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        services.AddPostgres<QuestionsDbContext>();
        services.AddScoped<IAnswersRepository, AnswersRepository>();
        services.AddScoped<IQuestionImportsRepository, QuestionImportsRepository>();
        services.AddScoped<IQuestionsRepository, QuestionsRepository>();
        services.AddScoped<IQuestionsModuleApi, QuestionsModuleApi>();
        services.AddSingleton<IQuestionImportFileStorage, QuestionImportFileStorage>();

        services.AddAzureClients(builder =>
        {
            if (hostEnvironment.IsDevelopment())
            {
                var connectionString = configuration["Azure:Storage:QuestionImports:LocalDevelopmentConnectionString"]!;

                builder
                    .AddBlobServiceClient(connectionString)
                    .WithName(BlobServiceClients.QuestionImports);

                return;
            }

            var questionImportsStorageAccountUri = new Uri(configuration["Azure:Storage:QuestionImports:ServiceUri"]!);

            builder
                .AddBlobServiceClient(questionImportsStorageAccountUri)
                .WithName(BlobServiceClients.QuestionImports)
                .WithCredential(new DefaultAzureCredential());
        });

        services.AddKeyedSingleton<BlobContainerClient>(
            BlobContainerClients.QuestionImports,
            (serviceProvider, _) =>
            {
                var factory = serviceProvider
                    .GetRequiredService<IAzureClientFactory<BlobServiceClient>>();

                var serviceClient = factory.CreateClient(
                    BlobServiceClients.QuestionImports);

                return serviceClient.GetBlobContainerClient(
                    BlobContainerNames.QuestionImports);
            }
        );
        
        return services;
    }
}
