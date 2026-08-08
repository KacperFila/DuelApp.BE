using Azure.Identity;
using Azure.Storage.Blobs;
using DuelApp.Modules.Questions.Application.Abstractions;
using DuelApp.Modules.Questions.Infrastructure.Configuration;
using DuelApp.Modules.Questions.Infrastructure.Const;
using DuelApp.Modules.Questions.Infrastructure.EF.Repositories;
using DuelApp.Modules.Questions.Infrastructure.Services;
using DuelApp.Modules.Questions.Shared;
using DuelApp.Shared.Abstractions.Messaging;
using DuelApp.Shared.Infrastructure.Postgres;
using DuelApp.Shared.Infrastructure.Messaging.ServiceBus;
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

        AddQuestionPublicationsServiceBus(
            services,
            configuration,
            hostEnvironment);

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

    private static void AddQuestionPublicationsServiceBus(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        var optionsSection = configuration.GetRequiredSection(
            QuestionPublicationsServiceBusOptions.SectionName);

        services
            .AddOptions<QuestionPublicationsServiceBusOptions>()
            .Bind(optionsSection)
            .Validate(
                options => hostEnvironment.IsDevelopment()
                    ? !string.IsNullOrWhiteSpace(options.ConnectionString)
                    : !string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace),
                hostEnvironment.IsDevelopment()
                    ? "ConnectionString must be configured for the Service Bus emulator."
                    : "FullyQualifiedNamespace must be configured for Azure Service Bus.")
            .ValidateOnStart();

        var options = optionsSection.Get<QuestionPublicationsServiceBusOptions>()!;

        services.AddAzureClients(builder =>
        {
            if (hostEnvironment.IsDevelopment())
            {
                builder.AddServiceBusClient(options.ConnectionString!);
                return;
            }

            builder
                .AddServiceBusClientWithNamespace(options.FullyQualifiedNamespace!)
                .WithCredential(new DefaultAzureCredential());
        });

        services.AddSingleton<IServiceBusMessagePublisher, ServiceBusMessagePublisher>();
    }
}
