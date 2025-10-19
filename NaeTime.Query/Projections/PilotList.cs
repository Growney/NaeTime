using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class PilotList
{
    private readonly ConcurrentDictionary<Guid, Pilot> _pilots = new();

    public void When(PilotRegistered e)
    {
        _pilots[e.PilotId] = new Pilot(e.PilotId, null, null, null, false);
    }

    public void When(PilotRenamed e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { Firstname = e.Firstname, Lastname = e.Lastname };
    }

    public void When(PilotCallsignAssigned e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { Callsign = e.CallSign };
    }

    public void When(PilotBindingPhraseChanged e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { HasBindingPhrase = true };
    }

    public void When(PilotBindingPhraseRemoved e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { HasBindingPhrase = false };
    }

    public IEnumerable<Pilot> GetAllPilots() => _pilots.Values;
    public Pilot? GetPilotById(Guid pilotId) => _pilots.TryGetValue(pilotId, out var pilot) ? pilot : null;
}
