using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;

namespace DuelApp.Shared.Infrastructure.Contexts;

public class ContextFactory : IContextFactory
{
    public IContext Create(HttpContext httpContext)
    {
        return new Context(httpContext);
    }
}