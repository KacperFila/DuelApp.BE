using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Duels.Application.Exceptions;

internal class DuelNotFoundException : DuelAppException
{
    public Guid DuelId { get; set;  }
    
    public DuelNotFoundException(Guid duelId) : base($"No duel found with id: {duelId}.")
    {
        DuelId = duelId;
    }
}