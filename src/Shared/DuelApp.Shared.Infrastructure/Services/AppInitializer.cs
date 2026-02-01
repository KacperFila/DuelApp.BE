using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DuelApp.Shared.Infrastructure.Services;

internal class AppInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppInitializer> _logger;
    private readonly IHostEnvironment _env;

    public AppInitializer(IServiceProvider serviceProvider, ILogger<AppInitializer> logger, IHostEnvironment env)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _env = env;
    }
        
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;
        _logger.LogInformation("[{Time}] Starting App Initializer in environment: {Environment}", timestamp, _env.EnvironmentName);
        
        if (_env.IsDevelopment())
        {
            var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(DbContext).IsAssignableFrom(x) && !x.IsInterface && x != typeof(DbContext))
                .ToList();

            _logger.LogInformation(
                "[{Time}] Found {Count} DbContext(s): {@DbContexts}",
                timestamp,
                dbContextTypes.Count,
                dbContextTypes.Select(t => t.FullName).ToList()
            );

            using var scope = _serviceProvider.CreateScope();
            foreach (var dbContextType in dbContextTypes)
            {
                var dbContext = scope.ServiceProvider.GetService(dbContextType) as DbContext;
                if (dbContext is null)
                {
                    _logger.LogWarning(
                        "[{Time}] DbContext {DbContextType} not resolved, skipping.",
                        DateTime.UtcNow,
                        dbContextType.FullName
                    );
                    continue;
                }

                try
                {
                    _logger.LogInformation(
                        "[{Time}] Applying migrations for {DbContextType} started.",
                        DateTime.UtcNow,
                        dbContextType.FullName
                    );

                    await dbContext.Database.MigrateAsync(cancellationToken);

                    _logger.LogInformation(
                        "[{Time}] Applying migrations for {DbContextType} finished successfully.",
                        DateTime.UtcNow,
                        dbContextType.FullName
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[{Time}] Applying migrations failed for {DbContextType}",
                        DateTime.UtcNow,
                        dbContextType.FullName
                    );
                    throw;
                }
            }
        }

        _logger.LogInformation("[{Time}] App Initializer finished.", DateTime.UtcNow);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}