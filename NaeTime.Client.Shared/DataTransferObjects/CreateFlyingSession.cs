namespace NaeTime.Client.Shared.DataTransferObjects;
public record CreateFlyingSession(string? Description, DateTime Start, DateTime ExpectedEnd, Guid TrackId);