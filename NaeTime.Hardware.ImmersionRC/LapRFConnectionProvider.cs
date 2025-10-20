using NaeTime.Hardware.ImmersionRC.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.ImmersionRC;
public class LapRFConnectionProvider : ILapRFConnectionProvider
{
    private readonly ConcurrentDictionary<Guid, ILapRFConnection> _connections = new();

    public ILapRFConnection? GetLapRFConnection(Guid timerId)
    {
        _connections.TryGetValue(timerId, out ILapRFConnection? connection);
        return connection;
    }

    public void SetLapRFConnection(Guid timerId, ILapRFConnection connection)
    {
        _connections.AddOrUpdate(timerId, connection, (key, oldValue) => connection);
    }
}
