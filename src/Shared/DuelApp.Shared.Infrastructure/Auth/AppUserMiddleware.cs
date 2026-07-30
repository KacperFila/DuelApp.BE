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
            var userId = context.Identity.UserId;
            var user = await _usersModuleApi.GetByUserIdAsync(userId)
                        ?? await _usersModuleApi.CreateAsync(
                            userId,
                            context.Identity.Claims);
            
            var newIdentity = context.Identity.WithProfileId(user.ProfileId);

            ContextAccessor.Set(
                new Context(context.TraceId, newIdentity));
        }

        await next(httpContext);
    }
}
