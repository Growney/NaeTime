using NaeTime.Timing.Models;

namespace NaeTime.Timing.OpenPractice.Leaderboards;
public class ConsecutiveLapLeaderboard
{
    private readonly Dictionary<Guid, FastestConsecutiveLaps> _pilotLaps = new();

    public void AddFastestConsecutiveLap(Guid pilotId, uint startLapNumber, uint endLapNumber, uint totalLaps, long totalMilliseconds, DateTime uastLapCompletionUtc)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, new FastestConsecutiveLaps(startLapNumber, endLapNumber, totalLaps, totalMilliseconds, uastLapCompletionUtc));
        }
        else
        {
            var existingFastest = _pilotLaps[pilotId];
            if (existingFastest.TotalLaps > totalLaps)
            {
                _pilotLaps[pilotId] = new FastestConsecutiveLaps(startLapNumber, endLapNumber, totalLaps, totalMilliseconds, uastLapCompletionUtc);
            }
            else if (existingFastest.TotalLaps == totalLaps)
            {
                if (existingFastest.TotalMilliseconds > totalMilliseconds)
                {
                    _pilotLaps[pilotId] = new FastestConsecutiveLaps(startLapNumber, endLapNumber, totalLaps, totalMilliseconds, uastLapCompletionUtc);
                }
            }
        }
    }

    public IEnumerable<ConsecutiveLapLeaderboardPosition> GetPositions()
    {
        var laps = _pilotLaps.ToList();
        laps.Sort((x, y) => CompareLaps(x.Value, x.Value));
        var positions = new List<ConsecutiveLapLeaderboardPosition>();
        for (int i = 0; i < laps.Count; i++)
        {
            var lap = laps[i];
            positions.Add(new ConsecutiveLapLeaderboardPosition((uint)i, lap.Key, lap.Value.StartLapNumber, lap.Value.EndLapNumber, lap.Value.TotalLaps, lap.Value.TotalMilliseconds));
        }
        return positions;
    }

    private int CompareLaps(FastestConsecutiveLaps a, FastestConsecutiveLaps b)
    {
        var lapCompare = a.TotalLaps.CompareTo(b.TotalLaps);
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
