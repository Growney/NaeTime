namespace NaeTime.Client.Shared.DataTransferObjects.Track;
public record TrackDetails(Guid Id, string Name, IEnumerable<TimedGateDetails> TimedGates);
