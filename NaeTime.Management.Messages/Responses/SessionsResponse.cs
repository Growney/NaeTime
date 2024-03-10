using static NaeTime.Management.Messages.Responses.SessionsResponse;

namespace NaeTime.Management.Messages.Responses;
public record SessionsResponse(IEnumerable<Session> Sessions)
{
    public record Session(Guid Id, string? Name, SessionType Type, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds);
    public enum SessionType
    {
        OpenPractice
    }
}
