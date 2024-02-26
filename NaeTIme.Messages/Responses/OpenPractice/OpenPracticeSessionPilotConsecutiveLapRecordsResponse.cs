namespace NaeTime.Messages.Responses;
public record OpenPracticeSessionPilotConsecutiveLapRecordsResponse(IEnumerable<OpenPracticeSessionPilotConsecutiveLapRecordsResponse.ConsecutiveLapRecord> Records)
{
    public record ConsecutiveLapRecord(uint LapCap, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
}
