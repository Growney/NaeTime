namespace NaeTime.Timing.Models;
public record FastestConsecutiveLaps(uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps) : IComparable<FastestConsecutiveLaps>
{
    public int CompareTo(FastestConsecutiveLaps? other)
    {
        if (other == null)
        {
            return 1;
        }

        //Other is compared to this here to provide a decending result where more laps is better
        var totalLapsComparison = other.TotalLaps.CompareTo(TotalLaps);
        if (totalLapsComparison != 0)
        {
            return totalLapsComparison;
        }

        var timeComparison = TotalMilliseconds.CompareTo(other.TotalMilliseconds);
        if (timeComparison != 0)
        {
            return timeComparison;
        }

        return LastLapCompletionUtc.CompareTo(other.LastLapCompletionUtc);
    }
}