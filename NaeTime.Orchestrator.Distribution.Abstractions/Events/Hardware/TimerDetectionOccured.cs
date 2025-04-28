namespace NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;
public record TimerDetectionOccured(Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
