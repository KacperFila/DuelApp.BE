using DuelApp.Modules.Matchmaking.Application.DTO;

namespace DuelApp.Modules.Matchmaking.Application.Services.Implementation;

public class MatchmakingManager : IMatchmakingManager
{
    private readonly Queue<Guid> _queue = new();

    public MatchmakingResultDto? JoinQueue(Guid userId)
    {
        if (_queue.Count == 0)
        {
            _queue.Enqueue(userId);
            return null;
        }

        var opponent = _queue.Dequeue();
        return new MatchmakingResultDto(opponent, userId);
    }
}