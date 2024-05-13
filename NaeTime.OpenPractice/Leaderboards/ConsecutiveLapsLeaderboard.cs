
namespace NaeTime.OpenPractice.Leaderboards;
public class ConsecutiveLapsLeaderboard
{
    private readonly Dictionary<Guid, ConsecutiveLapRecord> _pilotLaps = new();

    public bool SetFastest(Guid pilotId, ConsecutiveLapRecord laps)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, laps);
            return true;
        }
        else
        {
            ConsecutiveLapRecord existing = _pilotLaps[pilotId];
            if (existing != laps)
            {
                _pilotLaps[pilotId] = laps;
                return true;
            }
        }
        return false;
    }
    public IDictionary<Guid, ConsecutiveLapsLeaderboardPosition> GetPositions()
    {
        List<KeyValuePair<Guid, ConsecutiveLapRecord>> laps = _pilotLaps.ToList();
        laps.Sort((x, y) => CompareLaps(x.Value, y.Value));
        Dictionary<Guid, ConsecutiveLapsLeaderboardPosition> positions = new();
        for (int i = 0; i < laps.Count; i++)
        {
            KeyValuePair<Guid, ConsecutiveLapRecord> lap = laps[i];
            positions.Add(lap.Key, new ConsecutiveLapsLeaderboardPosition(i, lap.Key, lap.Value.TotalLaps, lap.Value.TotalMilliseconds, lap.Value.LastLapCompletionUtc, lap.Value.IncludedLaps));
        }

        return positions;
    }

    private int CompareLaps(ConsecutiveLapRecord a, ConsecutiveLapRecord b)
    {
        //B is compared to A here to provide a decending result where more laps is better
        int lapCompare = b.TotalLaps.CompareTo(a.TotalLaps);
        if (lapCompare != 0)
        {
            return lapCompare;
        }
        int timeCompare = a.TotalMilliseconds.CompareTo(b.TotalMilliseconds);
        if (timeCompare != 0)
        {
            return timeCompare;
        }
        return a.LastLapCompletionUtc.CompareTo(b.LastLapCompletionUtc);
    }
}
