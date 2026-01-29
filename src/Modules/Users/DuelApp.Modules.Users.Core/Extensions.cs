using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DuelApp.Modules.Users.Core.DAL.Repositories;
using DuelApp.Modules.Users.Core.DAL;
using DuelApp.Modules.Users.Core.Entities;
using DuelApp.Modules.Users.Core.Repositories;
using DuelApp.Modules.Users.Core.Services;
using System.Runtime.CompilerServices;
using DuelApp.Shared.Infrastructure.Postgres;

[assembly: InternalsVisibleTo("DuelApp.Modules.Users.Api")]

namespace DuelApp.Modules.Users.Core
{
    internal static class Extensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
            => services
                .AddScoped<IUserRepository, UserRepository>()
                .AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>()
                .AddTransient<IIdentityService, IdentityService>()
                .AddPostgres<UsersDbContext>();
    }
}