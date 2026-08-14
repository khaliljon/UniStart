using System.Collections.Concurrent;

namespace UniStart.Application.Services;

public class PresenceTracker
{
    private readonly ConcurrentDictionary<int, HashSet<string>> _onlineUsers = new();

    public bool UserConnected(int userId, string connectionId)
    {
        var isNewUser = false;
        _onlineUsers.AddOrUpdate(userId,
            _ =>
            {
                isNewUser = true;
                return new HashSet<string> { connectionId };
            },
            (_, existing) =>
            {
                lock (existing) { existing.Add(connectionId); }
                return existing;
            });
        return isNewUser;
    }

    public bool UserDisconnected(int userId, string connectionId)
    {
        if (!_onlineUsers.TryGetValue(userId, out var connections))
            return false;

        lock (connections)
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _onlineUsers.TryRemove(userId, out _);
                return true;
            }
        }
        return false;
    }

    public bool IsOnline(int userId) => _onlineUsers.ContainsKey(userId);

    public int[] GetOnlineUsers() => _onlineUsers.Keys.ToArray();
}
