namespace NaeTime.Messages.Events.Activation;
public record SessionActivated(Guid SessionId, SessionActivated.SessionType Type, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds)
{
    public enum SessionType
    {
        OpenPractice,
    }
}
