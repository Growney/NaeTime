namespace NaeTime.Hardware.Messages;
public record NodeTimerExitThresholdConfigured(Guid TimerId, byte Lane, ushort Threshold);

