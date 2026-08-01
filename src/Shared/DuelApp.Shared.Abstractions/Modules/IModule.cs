using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DuelApp.Shared.Abstractions.Modules
{
    public interface IModule
    {
        string Name { get; }
        string Path { get; }
        IEnumerable<string> Policies => null;
        void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment);
        void Use(IApplicationBuilder app);
    }
}