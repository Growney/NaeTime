namespace NaeTime.Messages.Responses;
public record ActiveSessionResponse(Guid SessionId, ActiveSessionResponse.SessionType Type, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, ActiveSessionResponse.Track ActiveTrack)
{
    public enum SessionType
    {
        OpenPractice,
    }
    public record Track(Guid TrackId, byte Lanes, IEnumerable<Guid> Timers);
}
