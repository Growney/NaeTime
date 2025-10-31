using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class PilotProjection : IPilotProjection
{
    private readonly ConcurrentDictionary<Guid, Pilot> _pilots = new();

    private void When(PilotRegistered e)
    {
        _pilots[e.PilotId] = new Pilot(e.PilotId, null, null, null, false);
    }

    private void When(PilotRenamed e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { Firstname = e.Firstname, Lastname = e.Lastname };
    }

    private void When(PilotCallsignAssigned e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { Callsign = e.CallSign };
    }

    private void When(PilotBindingPhraseChanged e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { HasBindingPhrase = true };
    }

    private void When(PilotBindingPhraseRemoved e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { HasBindingPhrase = false };
    }

    public IEnumerable<Pilot> GetAllPilots() => _pilots.Values;
    public Pilot? GetPilotById(Guid pilotId) => _pilots.TryGetValue(pilotId, out var pilot) ? pilot : null;
}
