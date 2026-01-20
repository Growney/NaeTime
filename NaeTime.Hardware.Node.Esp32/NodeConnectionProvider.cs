using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeConnectionProvider : INodeConnectionProvider
{
    private readonly ConcurrentDictionary<Guid, INodeConnection> _connections = new();

    public INodeConnection? GetNodeConnection(Guid timerId)
    {
        _connections.TryGetValue(timerId, out INodeConnection? connection);
        return connection;
    }

    public void SetNodeConnection(Guid timerId, INodeConnection connection)
    {
        _connections.AddOrUpdate(timerId, connection, (key, oldValue) => connection);
    }
}
