namespace NaeTime.OpenPractice.Messages.Events;
public record OpenPracticeDetectionMoved(Guid Id, ulong HardwareTime, long SoftwareTime, DateTime UtcTime);
