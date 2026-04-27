using DuelApp.Modules.Matchmaking.Application.Abstractions;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;

namespace DuelApp.Modules.Matchmaking.Application.Services.Implementations;

public sealed class MatchmakingService : IMatchmakingService
{
    private readonly IMatchmakingRepository _repository;

    public MatchmakingService(IMatchmakingRepository repository)
    {
        _repository = repository;
    }

    public async Task JoinQueueAsync(Guid playerId)
    {
        var entry = MatchmakingQueueEntry.Create(playerId);

        await _repository.AddAsync(entry);
    }

    public async Task LeaveQueueAsync(Guid playerId)
    {
        var entry = await _repository.GetByPlayerIdAsync(playerId);

        if (entry is null)
        {
            return;
        }
        
        entry.MarkAsMatched();
        await _repository.UpdateAsync(entry);
    }
}