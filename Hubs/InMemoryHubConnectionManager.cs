namespace SyncoraBackend.Hubs;

public class InMemoryHubConnectionManager
{
    private readonly Dictionary<int, HashSet<string>> _connections = new();

    public void AddConnection(int userId, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.ContainsKey(userId))
                _connections[userId] = new HashSet<string>();

            _connections[userId].Add(connectionId);
        }
    }

    public void RemoveConnection(int userId, string connectionId)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(userId, out var conns))
            {
                conns.Remove(connectionId);
                if (conns.Count == 0)
                    _connections.Remove(userId);
            }
        }
    }

    public IReadOnlyList<string> GetConnections(int userId)
    {
        lock (_connections)
        {
            return _connections.TryGetValue(userId, out var conns)
                ? conns.ToList()
                : [];
        }
    }
}