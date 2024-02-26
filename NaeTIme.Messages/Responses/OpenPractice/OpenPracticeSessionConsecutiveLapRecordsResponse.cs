namespace NaeTime.Messages.Responses;
public record OpenPracticeSessionConsecutiveLapRecordsResponse(IEnumerable<OpenPracticeSessionConsecutiveLapRecordsResponse.ConsecutiveLapRecord> Records)
{
    public record ConsecutiveLapRecord(Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
}
