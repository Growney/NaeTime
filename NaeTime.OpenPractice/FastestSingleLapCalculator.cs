using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Models;

namespace NaeTime.Timing;
public class FastestSingleLapCalculator
{
    public SingleLapRecord? Calculate(IEnumerable<Lap> laps)
    {
        if (!laps.Any(x => x.Status == LapStatus.Completed))
        {
            return null;
        }

        Guid lapId = Guid.Empty;
        long lapMilliseconds = long.MaxValue;
        DateTime finishedUtc = DateTime.MinValue;
        foreach (Lap lap in laps)
        {
            if (lap.Status != LapStatus.Completed)
            {
                continue;
            }

            if (lap.TotalMilliseconds < lapMilliseconds)
            {
                lapId = lap.LapId;
                lapMilliseconds = lap.TotalMilliseconds;
                finishedUtc = lap.FinishedUtc;
            }
        }

        return new SingleLapRecord(lapId, lapMilliseconds, finishedUtc);
    }
}
