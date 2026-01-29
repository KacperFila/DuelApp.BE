using DuelApp.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;

namespace DuelApp.Shared.Infrastructure.Contexts
{
    internal class ContextFactory : IContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContextFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public IContext Create()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            return httpContext is null ? Context.Empty : new Context(httpContext);
        }
    }
}