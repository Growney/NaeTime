namespace NaeTime.Client.Shared.DataTransferObjects.FlyingSession;
public record CreateFlyingSession(string? Description, DateTime Start, DateTime ExpectedEnd, Guid TrackId);