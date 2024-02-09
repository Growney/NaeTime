namespace NaeTime.Messages.Events.Timing;
public record LapCompleted(Guid SessionId, byte Lane, uint LapNumber, long SoftwareTime, DateTime UtcTime, ulong? HardwareTime, long TotalTime);