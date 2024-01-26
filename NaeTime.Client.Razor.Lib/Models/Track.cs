namespace NaeTime.Client.Razor.Lib.Models;
public class Track
{
    public Track(Guid id, string name, IEnumerable<Guid> timedGates)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _gates.AddRange(timedGates.Select(x => new TimedGate(x)));
    }

    public Guid Id { get; }
    public string Name { get; set; } = string.Empty;

    private readonly List<TimedGate> _gates = new();

    public IEnumerable<TimedGate> TimedGates => _gates;

    public void AddTimedGate(Guid timerId)
    {
        if (_gates.Any(x => x.TimerId == timerId))
        {
            throw new ArgumentException("Gate with same timer already exists", nameof(timerId));
        }

        _gates.Add(new TimedGate(timerId));
    }
    public void MoveTimedGateUp(Guid timerId)
    {
        var existingGateIndex = _gates.FindIndex(x => x.TimerId == timerId);

        if (existingGateIndex <= 0)
        {
            return;
        }

        var existingGate = _gates[existingGateIndex];
        _gates[existingGateIndex] = _gates[existingGateIndex - 1];
        _gates[existingGateIndex - 1] = existingGate;
    }
    public bool CanTimedGateMoveUp(Guid timerId)
    {
        var existingGateIndex = _gates.FindIndex(x => x.TimerId == timerId);

        return existingGateIndex > 0;
    }

    public void MoveTimedGateDown(Guid timerId)
    {
        var existingGateIndex = _gates.FindIndex(x => x.TimerId == timerId);

        if (existingGateIndex < 0 || existingGateIndex == _gates.Count - 1)
        {
            return;
        }

        var existingGate = _gates[existingGateIndex];
        _gates[existingGateIndex] = _gates[existingGateIndex + 1];
        _gates[existingGateIndex + 1] = existingGate;
    }

    public bool CanTimedGateMoveDown(Guid timerId)
    {
        var existingGateIndex = _gates.FindIndex(x => x.TimerId == timerId);

        return existingGateIndex >= 0 && existingGateIndex < _gates.Count - 1;
    }

    public void RemoveTimedGate(Guid timerId)
    {
        var gate = _gates.FirstOrDefault(x => x.TimerId == timerId);
        if (gate != null)
        {
            _gates.Remove(gate);
        }
    }
}
