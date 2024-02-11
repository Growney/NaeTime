using NaeTime.Timing.Models;

namespace NaeTime.Timing;
public class FastestSingleLapCalculator
{
    public FastestSingleLap? Calculate(IEnumerable<Lap> laps)
    {
        if (!laps.Any())
        {
            return null;
        }
        uint lapNumber = 0;
        long lapMilliseconds = long.MaxValue;
        DateTime finishedUtc = DateTime.MinValue;
        foreach (var lap in laps)
        {
            if (lap.TotalMilliseconds < lapMilliseconds)
            {
                lapNumber = lap.LapNumber;
                lapMilliseconds = lap.TotalMilliseconds;
                finishedUtc = lap.FinishedUtc;
            }
        }
        return new FastestSingleLap(lapNumber, lapMilliseconds, finishedUtc);
    }
}
