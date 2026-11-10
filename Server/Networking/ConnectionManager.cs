using System.Collections.Concurrent;

namespace Orvex.Server.Networking;

public sealed class ConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ClientConnection>
        _connections = new();

    public int Count => _connections.Count;

    public IReadOnlyCollection<ClientConnection> Connections =>
        _connections.Values.ToArray();

    public bool Add(ClientConnection connection)
    {
        return _connections.TryAdd(connection.Id, connection);
    }

    public bool Remove(Guid id, out ClientConnection? connection)
    {
        return _connections.TryRemove(id, out connection);
    }

    public bool TryGet(Guid id, out ClientConnection? connection)
    {
        return _connections.TryGetValue(id, out connection);
    }

    public void Clear()
    {
        foreach (var connection in _connections.Values)
        {
            _ = connection.DisposeAsync();
        }

        _connections.Clear();
    }
}