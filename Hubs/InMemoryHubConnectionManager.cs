
namespace SyncoraBackend.Hubs;

/// <summary>
///     In memory hub connection manager
///     Used to store the connections ids and device ids in relation to each user
/// </summary>
public class InMemoryHubConnectionManager
{
    // Dictionary<UserId, HashSet<ConnectionId, DeviceId>>
    private readonly Dictionary<int, HashSet<(string ConnectionId, string DeviceId)>> _connections = [];

    public void AddConnection(int userId, string connectionId, string deviceId)
    {
        lock (_connections)
        {
            if (!_connections.ContainsKey(userId))
                _connections[userId] = [];

            _connections[userId].Add((connectionId, deviceId));
        }
    }

    public void RemoveConnection(int userId, string connectionId)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(userId, out var conns))
            {
                conns.RemoveWhere(c => c.ConnectionId == connectionId);
                if (conns.Count == 0)
                    _connections.Remove(userId);
            }
        }
    }

    public IReadOnlyList<string> GetConnections(int userId, string? excludeDeviceId = null)
    {
        lock (_connections)
        {
            return _connections.TryGetValue(userId, out var conns)
                ? conns.Where(c => c.DeviceId != excludeDeviceId).Select(c => c.ConnectionId).ToList()
                : [];
        }
    }

    public IReadOnlyList<string> GetConnectionsForUsers(List<int> userIds, string? excludeDeviceId = null)
    {
        lock (_connections)
        {
            return [.. userIds.SelectMany(userId => _connections.TryGetValue(userId, out var conns) ? conns.Where(c => c.DeviceId != excludeDeviceId).Select(c => c.ConnectionId) : [])];
        }
    }

    internal string? GetConnectionIdForDevice(string deviceId)
    {
        lock (_connections)
        {
            return _connections.SelectMany(c => c.Value).Where(c => c.DeviceId == deviceId).Select(c => c.ConnectionId).FirstOrDefault();
        }

    }
}