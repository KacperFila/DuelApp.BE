using System;
using System.Collections.Generic;

namespace DuelApp.Shared.Abstractions.Contexts;

public interface IIdentityContext
{
    bool IsAuthenticated { get; }
    public Guid Id { get; }
    public string KeycloakUserId { get; }
    string Role { get; }
    Dictionary<string, IEnumerable<string>> Claims { get; }
    IIdentityContext WithUserId(Guid userId);
}