using NaeTime.Hardware.ELRS.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.ELRS;
public class BackpackConnectorProvider : IBackpackConnectorProvider
{
    private readonly ConcurrentDictionary<Guid, IBackpackConnector> _connectors = new();

    public IBackpackConnector? GetBackpackConnector(Guid id)
    {
        _connectors.TryGetValue(id, out IBackpackConnector? connector);
        return connector;
    }

    public void SetBackpackConnector(Guid id, IBackpackConnector connector)
    {
        _connectors.AddOrUpdate(id, connector, (key, oldValue) => connector);
    }
}
