using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;

namespace DuelApp.Shared.Infrastructure.Contexts;

public interface IContextFactory
{
    IContext Create(HttpContext httpContext);
}