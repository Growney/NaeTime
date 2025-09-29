namespace NaeTime.Events;
public enum SessionType
{
    OpenPractice,
}

public record ActiveSessionTrackingStarted(Guid TrackingId);
public record SessionActivated(Guid SessionId, SessionType SessionType);
public record SessionDeactivated(Guid SessionId, SessionType SessionType);
public record SessionScheduled(Guid SessionId, string Name, Guid TrackId, SessionType SessionType);
public record SessionRenamed(Guid Sessionid, string Name);