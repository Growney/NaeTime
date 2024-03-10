namespace NaeTime.OpenPractice.Messages.Events;
public record ConsecutiveLapRecordReduced(Guid SessionId, Guid PilotId, uint LapCap, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
