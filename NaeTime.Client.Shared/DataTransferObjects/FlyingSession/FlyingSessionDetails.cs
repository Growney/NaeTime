namespace NaeTime.Client.Shared.DataTransferObjects.FlyingSession;
public record FlyingSessionDetails(Guid Id, string Description, DateTime Start, DateTime ExpectedEnd, Guid TrackId);
