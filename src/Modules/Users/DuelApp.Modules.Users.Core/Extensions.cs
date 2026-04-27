using System.Runtime.CompilerServices;
using DuelApp.Modules.Users.Core.DAL;
using DuelApp.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("DuelApp.Modules.Users.Api")]

namespace DuelApp.Modules.Users.Core;

internal static class Extensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
        => services
            .AddPostgres<UsersDbContext>();
}