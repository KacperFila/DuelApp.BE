using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Duels.Application.Exceptions;

internal class NoRoundDetailsFoundException : DuelAppException
{
    public Guid DuelId { get; }
    public int RoundNumber { get; }
    
    public NoRoundDetailsFoundException(Guid duelId, int roundNumber) : base($"No round details found for round number: {roundNumber} for duel with Id: {duelId}")
    {
        DuelId = duelId;
        RoundNumber = roundNumber;
    }
}