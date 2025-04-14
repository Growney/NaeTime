namespace NaeTime.Persistence.Abstractions.Hardware;
public record TimerLaneConfiguredField(byte Lane, Guid TimerId, object? DesiredValue, object? ActualValue, TimerLaneField Field);
