namespace NaeTime.OpenPractice.Leaderboards;
public class SingleLapLeaderboard
{
    private readonly Dictionary<Guid, SingleLapRecord> _pilotLaps = new();

    public SingleLapLeaderboard()
    {
    }

    public bool SetFastest(Guid pilotId, Guid lapId, long totalMilliseconds, DateTime completionUtc)
    {
        SingleLapRecord newFastest = new(lapId, totalMilliseconds, completionUtc);

        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, newFastest);
            return true;
        }
        else
        {
            SingleLapRecord existing = _pilotLaps[pilotId];
            if (existing != newFastest)
            {
                _pilotLaps[pilotId] = new SingleLapRecord(lapId, totalMilliseconds, completionUtc);
                return true;
            }
        }

        return false;
    }

    public IDictionary<Guid, SingleLapLeaderboardPosition> GetPositions()
    {
        List<KeyValuePair<Guid, SingleLapRecord>> laps = _pilotLaps.ToList();
        laps.Sort((x, y) => CompareLaps(x.Value, y.Value));
        Dictionary<Guid, SingleLapLeaderboardPosition> positions = new();
        for (int i = 0; i < laps.Count; i++)
        {
            KeyValuePair<Guid, SingleLapRecord> lap = laps[i];
            positions.Add(lap.Key, new SingleLapLeaderboardPosition(i, lap.Key, lap.Value.LapId, lap.Value.LapMilliseconds, lap.Value.CompletionUtc));
        }

        return positions;
    }

    private int CompareLaps(SingleLapRecord a, SingleLapRecord b)
    {
        int timeCompare = a.LapMilliseconds.CompareTo(b.LapMilliseconds);
        if (timeCompare != 0)
        {
            return timeCompare;
        }
        return a.CompletionUtc.CompareTo(b.CompletionUtc);
    }
}
