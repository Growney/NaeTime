namespace NaeTime.OpenPractice.Leaderboards;
public record ConsecutiveLapRecord(uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps) : IComparable<ConsecutiveLapRecord>
{
    public int CompareTo(ConsecutiveLapRecord? other)
    {
        if (other == null)
        {
            return 1;
        }

        //We compare other first so that higher numbers are first
        int comparsion = other.TotalLaps.CompareTo(TotalLaps);
        if (comparsion != 0)
        {
            return comparsion;
        }

        comparsion = TotalMilliseconds.CompareTo(other.TotalMilliseconds);

        if (comparsion != 0)
        {
            return comparsion;
        }

        return LastLapCompletionUtc.CompareTo(other.LastLapCompletionUtc);

    }
}
