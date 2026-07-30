using System;
using System.Collections.Generic;

namespace DuelApp.Shared.Abstractions.Contexts;

public interface IIdentityContext
{
    bool IsAuthenticated { get; }
    public Guid ProfileId { get; }
    public Guid UserId { get; }
    string Role { get; }
    Dictionary<string, IEnumerable<string>> Claims { get; }
    IIdentityContext WithProfileId(Guid profileId);
}
