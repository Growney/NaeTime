namespace NaeTime.Messages.Events.Timing.Practice;
public record PracticeLapStarted(Guid SessionId, byte Lane, Guid? PilotId, uint LapNumber, long SoftwareTime, DateTime UtcTime, ulong? HardwareTime);