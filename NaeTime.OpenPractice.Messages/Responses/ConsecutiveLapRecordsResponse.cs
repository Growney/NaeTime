namespace NaeTime.OpenPractice.Messages.Responses;
public record ConsecutiveLapRecordsResponse(IEnumerable<ConsecutiveLapRecordsResponse.ConsecutiveLapRecord> Records)
{
    public record ConsecutiveLapRecord(Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
}
