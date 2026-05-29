using System.Threading.Tasks;
using DuelApp.Modules.Users.Shared;
using DuelApp.Shared.Abstractions.Contexts;
using DuelApp.Shared.Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;

namespace DuelApp.Shared.Infrastructure.Auth;

public class AppUserMiddleware : IMiddleware
{
    private readonly IUsersModuleApi _usersModuleApi;
    private readonly IContextAccessor _contextAccessor;
    
    public AppUserMiddleware(
        IUsersModuleApi usersModuleApi,
        IContextAccessor contextAccessor)
    {
        _usersModuleApi = usersModuleApi;
        _contextAccessor = contextAccessor;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var context = _contextAccessor.Current;
        
        if (context?.Identity?.IsAuthenticated == true)
        {
            var keycloakId = context.Identity.KeycloakUserId;
            var user =  await _usersModuleApi.GetByKeycloakIdAsync(keycloakId) 
                        ?? await _usersModuleApi.CreateAsync(
                            keycloakId,
                            context.Identity.Claims);
            
            var newIdentity = context.Identity.WithUserId(user.Id);

            ContextAccessor.Set(
                new Context(context.TraceId, newIdentity));
        }

        await next(httpContext);
    }
}