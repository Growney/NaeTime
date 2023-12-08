namespace NaeTime.Client.Shared.DataTransferObjects;
public record CreateTrack(string? Name, IEnumerable<TimedGate> TimedGates);