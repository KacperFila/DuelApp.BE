using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DuelApp.Shared.Abstractions.Contexts;

namespace DuelApp.Shared.Infrastructure.Contexts;

internal class IdentityContext : IIdentityContext
{
    public bool IsAuthenticated { get; }
    public Guid Id { get; }
    public string KeycloakUserId { get; }
    public string Email { get; }
    public string Role { get; }
    public Dictionary<string, IEnumerable<string>> Claims { get; }
        
    public IdentityContext(ClaimsPrincipal principal)
    {
        IsAuthenticated = principal.Identity?.IsAuthenticated is true;

        var userIdClaim =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
            
        KeycloakUserId = IsAuthenticated ? userIdClaim : string.Empty;
        Email = principal.FindFirst(ClaimTypes.Email)?.Value;
        Role = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        Claims = principal.Claims
            .GroupBy(x => x.Type)
            .ToDictionary(x => x.Key, x => x.Select(c => c.Value.ToString()));
    }

    public IdentityContext(bool isAuthenticated, Guid id, string keycloakUserId, string role, Dictionary<string, IEnumerable<string>> claims)
    {
        IsAuthenticated = isAuthenticated;
        Id = id;
        KeycloakUserId = keycloakUserId;
        Role = role;
        Claims = claims;
    }
    
    public IIdentityContext WithUserId(Guid userId)
        => new IdentityContext(IsAuthenticated, userId, KeycloakUserId, Role, Claims);
}