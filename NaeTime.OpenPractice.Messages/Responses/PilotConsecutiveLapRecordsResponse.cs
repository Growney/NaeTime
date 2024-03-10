namespace NaeTime.OpenPractice.Messages.Responses;
public record PilotConsecutiveLapRecordsResponse(IEnumerable<PilotConsecutiveLapRecordsResponse.ConsecutiveLapRecord> Records)
{
    public record ConsecutiveLapRecord(uint LapCap, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
}
