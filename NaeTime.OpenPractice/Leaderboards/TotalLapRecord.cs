namespace NaeTime.OpenPractice.Leaderboards;
public record TotalLapRecord(int TotalLaps, DateTime FirstLapCompletionUtc) : IComparable<TotalLapRecord>
{
    public int CompareTo(TotalLapRecord? other)
    {
        if (other == null)
        {
            return 1;
        }

        int comparison = TotalLaps.CompareTo(other.TotalLaps);
        if (comparison != 0)
        {
            return comparison;
        }

        return FirstLapCompletionUtc.CompareTo(other.FirstLapCompletionUtc);
    }
}
