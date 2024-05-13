namespace NaeTime.Client.Razor.Lib.Models;
public class Track
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    private readonly List<Guid> _timers = new();
    public IEnumerable<Guid> Timers => _timers;
    public long MinimumLapTimeMilliseconds { get; set; }
    public long? MaximumLapTimeMilliseconds { get; set; }

    public void AddTimer(Guid timerId)
    {
        _timers.Add(timerId);
    }
    public void AddTimers(IEnumerable<Guid> timerIds)
    {
        _timers.AddRange(timerIds);
    }
    public bool CanTimerMoveUp(Guid timerId)
    {
        int index = _timers.IndexOf(timerId);
        if (index == -1 || index == 0)
        {
            return false;
        }

        return true;
    }
    public void MoveTimerUp(Guid timerId)
    {
        int index = _timers.IndexOf(timerId);
        if (index == -1 || index == 0)
        {
            return;
        }

        _timers.RemoveAt(index);
        _timers.Insert(index - 1, timerId);
    }
    public bool CanTimerMoveDown(Guid timerId)
    {
        int index = _timers.IndexOf(timerId);
        if (index == -1 || index == _timers.Count - 1)
        {
            return false;
        }

        return true;
    }
    public void MoveTimerDown(Guid timerId)
    {
        int index = _timers.IndexOf(timerId);
        if (index == -1 || index == _timers.Count - 1)
        {
            return;
        }

        _timers.RemoveAt(index);
        _timers.Insert(index + 1, timerId);
    }
    public void RemoveTimer(Guid timerId)
    {
        _timers.Remove(timerId);
    }
}
