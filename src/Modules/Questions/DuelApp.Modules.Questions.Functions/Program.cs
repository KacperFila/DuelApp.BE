using DuelApp.Modules.Questions.Application;
using DuelApp.Modules.Questions.Infrastructure;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration, context.HostingEnvironment);
    })
    .Build();

await host.RunAsync();
