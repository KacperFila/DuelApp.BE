using System;
using System.Runtime.CompilerServices;
using Azure.Identity;
using Azure.Storage.Blobs;
using DuelApp.Modules.Users.Core.Constants;
using DuelApp.Modules.Users.Core.DAL;
using DuelApp.Modules.Users.Core.DAL.Repositories;
using DuelApp.Modules.Users.Core.Repositories;
using DuelApp.Modules.Users.Core.Services;
using DuelApp.Modules.Users.Shared;
using DuelApp.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("DuelApp.Modules.Users.Api")]

namespace DuelApp.Modules.Users.Core;

internal static class Extensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services.AddPostgres<UsersDbContext>();
        services.AddScoped<IUsersModuleApi, UsersModuleApi>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IAvatarStorageService, AvatarStorageService>();
        services.AddScoped<IAccountService, AccountService>();

        services.AddAzureClients(builder =>
        {
            if (hostEnvironment.IsDevelopment())
            {
                var connectionString = configuration["Azure:Storage:ProfilePictures:LocalDevelopmentConnectionString"]
                                       ?? throw new InvalidOperationException("Azure:Storage:ProfilePictures:LocalDevelopmentConnectionString configuration is required.");

                builder
                    .AddBlobServiceClient(connectionString)
                    .WithName(BlobServiceClients.ProfilePictures);

                return;
            }

            var profilePicturesStorageAccountUri = new Uri(configuration["Azure:Storage:ProfilePictures:ServiceUri"]!);

            builder
                .AddBlobServiceClient(profilePicturesStorageAccountUri)
                .WithName(BlobServiceClients.ProfilePictures)
                .WithCredential(new DefaultAzureCredential());
        });

        services.AddKeyedSingleton<BlobServiceClient>(
            BlobServiceClients.ProfilePictures,
            (serviceProvider, _) =>
            {
                var factory = serviceProvider
                    .GetRequiredService<IAzureClientFactory<BlobServiceClient>>();

                return factory.CreateClient(BlobServiceClients.ProfilePictures);
            });

        services.AddKeyedSingleton<BlobContainerClient>(
            BlobContainerClients.ProfilePictures,
            (serviceProvider, _) =>
            {
                var serviceClient = serviceProvider
                    .GetRequiredKeyedService<BlobServiceClient>(
                        BlobServiceClients.ProfilePictures);

                return serviceClient.GetBlobContainerClient(
                    BlobContainerNames.ProfilePictures);
            });

        return services;
    }
    
}
