using NaeTime.Timing.Models;

namespace NaeTime.Timing.OpenPractice.Leaderboards;
public class SingleLapLeaderboard
{
    private readonly Dictionary<Guid, FastestSingleLap> _pilotLaps = new();
    public Guid LeaderboardId { get; }

    public SingleLapLeaderboard(Guid leaderboardId)
    {
        LeaderboardId = leaderboardId;
    }

    public bool UpdateIfFaster(Guid pilotId, uint lapNumber, long totalMilliseconds, DateTime completionUtc)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, new FastestSingleLap(lapNumber, totalMilliseconds, completionUtc));
            return true;
        }
        else
        {
            if (_pilotLaps[pilotId].LapMilliseconds > totalMilliseconds)
            {
                _pilotLaps[pilotId] = new FastestSingleLap(lapNumber, totalMilliseconds, completionUtc);
                return true;
            }
        }
        return false;
    }
    public void RemovePilot(Guid pilotId)
    {
        _pilotLaps.Remove(pilotId);
    }
    public bool SetFastest(Guid pilotId, uint lapNumber, long totalMilliseconds, DateTime completionUtc)
    {
        var newFastest = new FastestSingleLap(lapNumber, totalMilliseconds, completionUtc);

        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, newFastest);
            return true;
        }
        else
        {
            var existing = _pilotLaps[pilotId];
            if (existing != newFastest)
            {
                _pilotLaps[pilotId] = new FastestSingleLap(lapNumber, totalMilliseconds, completionUtc);
                return true;
            }
        }
        return false;
    }

    public IEnumerable<SingleLapLeaderboardPosition> GetPositions()
    {
        var laps = _pilotLaps.ToList();
        laps.Sort((x, y) => CompareLaps(x.Value, y.Value));
        var positions = new List<SingleLapLeaderboardPosition>();
        for (int i = 0; i < laps.Count; i++)
        {
            var lap = laps[i];
            positions.Add(new SingleLapLeaderboardPosition((uint)i, lap.Key, lap.Value.LapNumber, lap.Value.LapMilliseconds, lap.Value.CompletionUtc));
        }
        return positions;
    }

    private int CompareLaps(FastestSingleLap a, FastestSingleLap b)
    {
        int timeCompare = a.LapMilliseconds.CompareTo(b.LapMilliseconds);
        if (timeCompare != 0)
        {
            return timeCompare;
        }
        return a.CompletionUtc.CompareTo(b.CompletionUtc);
    }
}
