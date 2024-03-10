using NaeTime.Timing.Models;

namespace NaeTime.OpenPractice.Leaderboards;
public class ConsecutiveLapsLeaderboard
{
    private readonly Dictionary<Guid, FastestConsecutiveLaps> _pilotLaps = new();
    public Guid LeaderboardId { get; }
    public uint LapCount { get; }
    public ConsecutiveLapsLeaderboard(Guid leaderboardId, uint lapCount)
    {
        LeaderboardId = leaderboardId;
        LapCount = lapCount;
    }
    public bool UpdateIfFaster(Guid pilotId, FastestConsecutiveLaps laps)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, laps);
            return true;
        }
        else
        {
            var existingFastest = _pilotLaps[pilotId];
            if (existingFastest.TotalLaps < laps.TotalLaps)
            {
                _pilotLaps[pilotId] = laps;
                return true;
            }
            else if (existingFastest.TotalLaps == laps.TotalLaps)
            {
                if (existingFastest.TotalMilliseconds > laps.TotalMilliseconds)
                {
                    _pilotLaps[pilotId] = laps;
                    return true;
                }
            }
        }
        return false;
    }
    public void RemovePilot(Guid pilotId)
    {
        _pilotLaps.Remove(pilotId);
    }
    public bool SetFastest(Guid pilotId, FastestConsecutiveLaps laps)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, laps);
            return true;
        }
        else
        {
            var existing = _pilotLaps[pilotId];
            if (existing != laps)
            {
                _pilotLaps[pilotId] = laps;
                return true;
            }
        }
        return false;
    }
    public IEnumerable<ConsecutiveLapsLeaderboardPosition> GetPositions()
    {
        var laps = _pilotLaps.ToList();
        laps.Sort((x, y) => CompareLaps(x.Value, y.Value));
        var positions = new List<ConsecutiveLapsLeaderboardPosition>();
        for (int i = 0; i < laps.Count; i++)
        {
            var lap = laps[i];
            positions.Add(new ConsecutiveLapsLeaderboardPosition((uint)i, lap.Key, lap.Value.TotalLaps, lap.Value.TotalMilliseconds, lap.Value.LastLapCompletionUtc, lap.Value.IncludedLaps));
        }
        return positions;
    }

    private int CompareLaps(FastestConsecutiveLaps a, FastestConsecutiveLaps b)
    {
        //B is compared to A here to provide a decending result where more laps is better
        var lapCompare = b.TotalLaps.CompareTo(a.TotalLaps);
        if (lapCompare != 0)
        {
            return lapCompare;
        }
        var timeCompare = a.TotalMilliseconds.CompareTo(b.TotalMilliseconds);
        if (timeCompare != 0)
        {
            return timeCompare;
        }
        return a.LastLapCompletionUtc.CompareTo(b.LastLapCompletionUtc);
    }
}
