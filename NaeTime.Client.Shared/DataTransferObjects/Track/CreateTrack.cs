namespace NaeTime.Client.Shared.DataTransferObjects.Track;
public record CreateTrack(string? Name, IEnumerable<TimedGateDetails> TimedGates);