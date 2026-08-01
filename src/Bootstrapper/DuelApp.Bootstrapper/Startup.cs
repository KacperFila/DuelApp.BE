using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DuelApp.Shared.Abstractions.Modules;
using DuelApp.Shared.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DuelApp.Bootstrapper
{
    public class Startup
    {
        private readonly IList<IModule> _modules;
        private readonly IList<Assembly> _assemblies;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            _assemblies = ModuleLoader.LoadAssemblies(configuration);
            _modules = ModuleLoader.LoadModules(_assemblies);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructure(_assemblies, _modules, _configuration);

            foreach (var module in _modules)
            {
                module.Register(services, _configuration, _environment);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IConfiguration configuration)
        {
            app.UseInfrastructure();

            foreach (var module in _modules)
            {
                module.Use(app);
            }

            logger.LogInformation($"Modules: {string.Join(", ", _modules.Select(x => x.Name))}");

            _assemblies.Clear();
            _modules.Clear();
        }
    }
}
