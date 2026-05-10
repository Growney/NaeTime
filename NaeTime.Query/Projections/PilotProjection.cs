using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class PilotProjection : IPilotProjection
{
    private class PilotSnapshot
    {
        public Guid PilotId { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Callsign { get; set; }
        public bool HasBindingPhrase { get; set; }
    }

    private class ProjectionSnapshot
    {
        public Dictionary<Guid, PilotSnapshot> Pilots { get; set; } = new();
        public Dictionary<Guid, byte[]> BindingPhrases { get; set; } = new();
    }

    private readonly ConcurrentDictionary<Guid, Pilot> _pilots = new();
    private readonly ConcurrentDictionary<Guid, byte[]> _bindingPhrases = new();

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

        _bindingPhrases[e.PilotId] = e.BindingPhrase;
    }

    private void When(PilotBindingPhraseRemoved e)
    {
        if (_pilots.TryGetValue(e.PilotId, out var pilot))
            _pilots[e.PilotId] = pilot with { HasBindingPhrase = false };

        _bindingPhrases.TryRemove(e.PilotId, out _);
    }

    public IEnumerable<Pilot> GetAllPilots() => _pilots.Values;
    public Pilot? GetPilotById(Guid pilotId) => _pilots.TryGetValue(pilotId, out var pilot) ? pilot : null;
    public byte[]? GetPilotBindingPhrase(Guid pilotId) => _bindingPhrases.TryGetValue(pilotId, out var phrase) ? phrase : null;

    private static PilotSnapshot ToSnapshot(Pilot pilot) =>
        new()
        {
            PilotId = pilot.Id,
            Firstname = pilot.Firstname,
            Lastname = pilot.Lastname,
            Callsign = pilot.Callsign,
            HasBindingPhrase = pilot.HasBindingPhrase,
        };

    private static Pilot FromSnapshot(PilotSnapshot snapshot) =>
        new(snapshot.PilotId, snapshot.Firstname, snapshot.Lastname, snapshot.Callsign, snapshot.HasBindingPhrase);

    private ProjectionSnapshot Snapshot()
    {
        var snapshot = new ProjectionSnapshot();

        foreach (var (pilotId, pilot) in _pilots)
        {
            snapshot.Pilots[pilotId] = ToSnapshot(pilot);
        }

        foreach (var (pilotId, phrase) in _bindingPhrases)
        {
            snapshot.BindingPhrases[pilotId] = (byte[])phrase.Clone();
        }

        return snapshot;
    }

    private void Restore(ProjectionSnapshot snapshot)
    {
        _pilots.Clear();
        _bindingPhrases.Clear();

        foreach (var (pilotId, pilotSnapshot) in snapshot.Pilots)
        {
            _pilots[pilotId] = FromSnapshot(pilotSnapshot);
        }

        foreach (var (pilotId, phrase) in snapshot.BindingPhrases)
        {
            _bindingPhrases[pilotId] = (byte[])phrase.Clone();
        }
    }
}
