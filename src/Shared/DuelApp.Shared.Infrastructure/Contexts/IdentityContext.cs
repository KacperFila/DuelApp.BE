using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DuelApp.Shared.Abstractions.Contexts;

namespace DuelApp.Shared.Infrastructure.Contexts;

internal class IdentityContext : IIdentityContext
{
    public bool IsAuthenticated { get; }
    public Guid ProfileId { get; }
    public Guid UserId { get; }
    public string Email { get; }
    public string Role { get; }
    public Dictionary<string, IEnumerable<string>> Claims { get; }
        
    public IdentityContext(ClaimsPrincipal principal)
    {
        IsAuthenticated = principal.Identity?.IsAuthenticated is true;

        var userIdClaim =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
            
        UserId = IsAuthenticated
            ? Guid.Parse(userIdClaim ?? throw new InvalidOperationException("Authenticated user does not have an identifier claim."))
            : Guid.Empty;
        Email = principal.FindFirst(ClaimTypes.Email)?.Value;
        Role = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        Claims = principal.Claims
            .GroupBy(x => x.Type)
            .ToDictionary(x => x.Key, x => x.Select(c => c.Value.ToString()));
    }

    public IdentityContext(bool isAuthenticated, Guid profileId, Guid userId, string role, Dictionary<string, IEnumerable<string>> claims)
    {
        IsAuthenticated = isAuthenticated;
        ProfileId = profileId;
        UserId = userId;
        Role = role;
        Claims = claims;
    }
    
    public IIdentityContext WithProfileId(Guid profileId)
        => new IdentityContext(IsAuthenticated, profileId, UserId, Role, Claims);
}
