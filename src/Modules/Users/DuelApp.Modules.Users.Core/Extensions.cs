using System.Runtime.CompilerServices;
using DuelApp.Modules.Users.Core.DAL;
using DuelApp.Modules.Users.Core.DAL.Repositories;
using DuelApp.Modules.Users.Core.Repositories;
using DuelApp.Modules.Users.Core.Services;
using DuelApp.Modules.Users.Shared;
using DuelApp.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("DuelApp.Modules.Users.Api")]

namespace DuelApp.Modules.Users.Core;

internal static class Extensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddPostgres<UsersDbContext>();
        services.AddScoped<IUsersModuleApi, UsersModuleApi>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IAvatarStorageService, AvatarStorageService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
    
}