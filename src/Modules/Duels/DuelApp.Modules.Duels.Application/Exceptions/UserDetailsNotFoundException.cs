using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Duels.Application.Exceptions;

public class UserDetailsNotFoundException : DuelAppException
{
    public Guid PlayerId { get; }
    
    public UserDetailsNotFoundException(Guid playerId) : base($"User details not found for userId: {playerId}")
    {
        PlayerId = playerId;
    }
}