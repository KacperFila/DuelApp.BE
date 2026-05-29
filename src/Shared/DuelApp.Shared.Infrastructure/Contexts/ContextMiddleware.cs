using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DuelApp.Shared.Infrastructure.Contexts;

internal class ContextMiddleware : IMiddleware
{
    private readonly IContextFactory _factory;

    public ContextMiddleware(IContextFactory factory)
    {
        _factory = factory;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var context = _factory.Create(httpContext);

        ContextAccessor.Set(context);

        await next(httpContext);
    }
}