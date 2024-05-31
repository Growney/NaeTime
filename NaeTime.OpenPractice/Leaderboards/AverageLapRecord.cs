namespace NaeTime.OpenPractice.Leaderboards;
public record AverageLapRecord(double AverageMilliseconds, DateTime FirstLapCompletionUtc) : IComparable<AverageLapRecord>
{
    public int CompareTo(AverageLapRecord? other)
    {
        if (other == null)
        {
            return 1;
        }

        int comparison = AverageMilliseconds.CompareTo(other.AverageMilliseconds);
        if (comparison != 0)
        {
            return comparison;
        }

        return FirstLapCompletionUtc.CompareTo(other.FirstLapCompletionUtc);
    }
}
