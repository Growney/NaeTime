namespace NaeTime.Hardware.Messages;
public record NodeTimerEntryThresholdConfigured(Guid TimerId, byte Lane, ushort Threshold);