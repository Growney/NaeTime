namespace NaeTime.Management.Messages.Responses;
public record SessionResponse(Guid Id, string? Name, SessionResponse.SessionType Type, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds)
{
    public enum SessionType
    {
        OpenPractice
    }
}
