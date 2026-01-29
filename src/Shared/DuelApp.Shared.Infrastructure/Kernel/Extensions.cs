using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DuelApp.Shared.Abstractions.Kernel;

namespace DuelApp.Shared.Infrastructure.Kernel
{
    internal static class Extensions
    {
        public static IServiceCollection AddDomainEvents(this IServiceCollection services,
            IEnumerable<Assembly> assemblies)
        {
            services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
            services.Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }
    }
}