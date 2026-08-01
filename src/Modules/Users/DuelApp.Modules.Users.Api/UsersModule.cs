using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using DuelApp.Modules.Users.Core;
using DuelApp.Shared.Abstractions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DuelApp.Modules.Users.Api
{
        internal class UsersModule : IModule
        {
            public const string BasePath = "users";
            public string Name { get; } = "Users";
            public string Path => BasePath;

            public IEnumerable<string> Policies { get; } = new[]
            {
                "users"
            };

            public void Register(
                IServiceCollection services,
                IConfiguration configuration,
                IHostEnvironment hostEnvironment)
            {
                services.AddCore(configuration, hostEnvironment);
            }

            public void Use(IApplicationBuilder app)
            {
            }
        }
    }
