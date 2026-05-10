using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;

public class InterfaceProjection : IInterfaceProjection
{
    private readonly ConcurrentDictionary<Guid, Interface> _interfaces = new();

    private void When(ELRSBackpackInterfaceAdded e)
    {
        _interfaces[e.Id] = new Interface(e.Id, e.Name, InterfaceType.ELRSBackpack);
    }

    private void When(ELRSBackpackInterfaceRenamed e)
    {
        if (_interfaces.TryGetValue(e.Id, out var hardwareInterface))
            _interfaces[e.Id] = hardwareInterface with { Name = e.Name };
    }

    public Interface? GetInterface(Guid id) => _interfaces.TryGetValue(id, out var iface) ? iface : null;
    public IEnumerable<Interface> GetInterfaces() => _interfaces.Values;
    public IEnumerable<Interface> GetInterfaces(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            if (_interfaces.TryGetValue(id, out var hardwareInterface))
            {
                yield return hardwareInterface;
            }
        }
    }
}
