using NaeTime.Timing.Models;

namespace NaeTime.OpenPractice.Leaderboards;
public class SingleLapLeaderboard
{
    private readonly Dictionary<Guid, FastestSingleLap> _pilotLaps = new();

    public SingleLapLeaderboard()
    {
    }

    public bool SetFastest(Guid pilotId, Guid lapId, long totalMilliseconds, DateTime completionUtc)
    {
        var newFastest = new FastestSingleLap(lapId, totalMilliseconds, completionUtc);

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
                _pilotLaps[pilotId] = new FastestSingleLap(lapId, totalMilliseconds, completionUtc);
                return true;
            }
        }

        return false;
    }

    public IDictionary<Guid, SingleLapLeaderboardPosition> GetPositions()
    {
        var laps = _pilotLaps.ToList();
        laps.Sort((x, y) => CompareLaps(x.Value, y.Value));
        var positions = new Dictionary<Guid, SingleLapLeaderboardPosition>();
        for (int i = 0; i < laps.Count; i++)
        {
            var lap = laps[i];
            positions.Add(lap.Key, new SingleLapLeaderboardPosition(i, lap.Key, lap.Value.LapId, lap.Value.LapMilliseconds, lap.Value.CompletionUtc));
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
