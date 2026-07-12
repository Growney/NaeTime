using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;

public class ELRSBackpackProjection : IELRSBackpackProjection
{
    private readonly ConcurrentDictionary<Guid, SerialELRSBackpackInterface> _elrsBackpacks = new();

    private void When(ELRSBackpackInterfaceAdded e)
    {
        _elrsBackpacks[e.Id] = new SerialELRSBackpackInterface(e.Id, e.Name, e.ComPort);
    }

    private void When(ELRSBackpackInterfaceRenamed e)
    {
        if (_elrsBackpacks.TryGetValue(e.Id, out var backpack))
            _elrsBackpacks[e.Id] = backpack with { Name = e.Name };
    }

    private void When(ELRSBackpackInterfaceComPortReconfigured e)
    {
        if (_elrsBackpacks.TryGetValue(e.Id, out var backpack))
            _elrsBackpacks[e.Id] = backpack with { ComPort = e.ComPort };
    }

    public SerialELRSBackpackInterface? GetSerialELRSBackpackInterface(Guid id) =>
        _elrsBackpacks.TryGetValue(id, out var backpack) ? backpack : null;

    public IEnumerable<SerialELRSBackpackInterface> GetAllSerialELRSBackpackInterfaces() => _elrsBackpacks.Values;
}
