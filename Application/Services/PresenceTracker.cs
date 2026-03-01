using System.Collections.Concurrent;

namespace UniStart.Application.Services;

/// <summary>
/// In-memory tracker for online user presence. Registered as Singleton.
/// Tracks userId → set of connectionIds (a user can have multiple tabs).
/// </summary>
public class PresenceTracker
{
    private readonly ConcurrentDictionary<int, HashSet<string>> _onlineUsers = new();

    /// <summary>Add a SignalR connection for a user.</summary>
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

    /// <summary>Remove a SignalR connection. Returns true if user has no more connections (went offline).</summary>
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
                return true; // user fully offline
            }
        }
        return false;
    }

    /// <summary>Check if a user is currently online.</summary>
    public bool IsOnline(int userId) => _onlineUsers.ContainsKey(userId);

    /// <summary>Get all currently online user IDs.</summary>
    public int[] GetOnlineUsers() => _onlineUsers.Keys.ToArray();
}
