using System.Collections.Concurrent;

namespace DuelApp.Modules.Duels.Infrastructure.Realtime;

public class DuelSessionManager
{
    private readonly ConcurrentDictionary<Guid, DuelSession> _sessions = new();
    private readonly ConcurrentDictionary<Guid, Guid> _userToDuel = new();

    public void Create(Guid duelId, Guid player1Id, Guid player2Id)
    {
        var session = new DuelSession(duelId, player1Id, player2Id);

        _sessions[duelId] = session;
        _userToDuel[player1Id] = duelId;
        _userToDuel[player2Id] = duelId;
    }

    public DuelSession GetSession(Guid duelId)
        => _sessions[duelId];

    public Guid? GetDuelId(Guid userId)
        => _userToDuel.TryGetValue(userId, out var duelId) ? duelId : null;

    public void Remove(Guid duelId)
    {
        if (_sessions.TryRemove(duelId, out var session))
        {
            _userToDuel.TryRemove(session.Player1Id, out _);
            _userToDuel.TryRemove(session.Player2Id, out _);
        }
    }
}