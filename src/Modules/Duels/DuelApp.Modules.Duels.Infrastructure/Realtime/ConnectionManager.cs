using System.Collections.Concurrent;

namespace DuelApp.Modules.Duels.Infrastructure.Realtime;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<Guid, string> _userToConnection = new();
    private readonly ConcurrentDictionary<string, Guid> _connectionToUser = new();

    public void Add(Guid userId, string connectionId)
    {
        _userToConnection[userId] = connectionId;
        _connectionToUser[connectionId] = userId;
    }

    public void Remove(Guid userId)
    {
        if (_userToConnection.TryRemove(userId, out var connId))
        {
            _connectionToUser.TryRemove(connId, out _);
        }
    }

    public string? GetConnectionId(Guid userId)
        => _userToConnection.TryGetValue(userId, out var conn) ? conn : null;

    public Guid? GetUserId(string connectionId)
        => _connectionToUser.TryGetValue(connectionId, out var user) ? user : null;
}