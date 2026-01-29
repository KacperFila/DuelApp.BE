using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DuelApp.Shared.Abstractions.Queries;

namespace DuelApp.Shared.Infrastructure.Queries
{
    internal static class Extensions
    {
        public static IServiceCollection AddQueries(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            services.Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }
    }
}