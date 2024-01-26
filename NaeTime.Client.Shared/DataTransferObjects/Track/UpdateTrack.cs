namespace NaeTime.Client.Shared.DataTransferObjects.Track;
public record UpdateTrack(Guid Id, string Name, IEnumerable<TimedGateDetails> TimedGates);
