namespace NaeTime.Persistence.Abstractions.Timing;
public record Lap(Guid Id, Guid SessionId, Guid PilotId, Detection EntryDetection, Detection? ExitDetection, LapStatus Status);