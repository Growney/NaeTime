namespace NaeTime.OpenPractice.Leaderboards;
public record SingleLapRecord(Guid LapId, long LapMilliseconds, DateTime CompletionUtc) : IComparable<SingleLapRecord>
{
    public int CompareTo(SingleLapRecord? other)
    {
        if (other == null)
        {
            return 1;
        }

        int result = LapMilliseconds.CompareTo(other.LapMilliseconds);

        if (result != 0)
        {
            return result;
        }

        return CompletionUtc.CompareTo(other.CompletionUtc);
    }
}

