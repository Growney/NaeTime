using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class ActiveSession : AggregateRoot
{
    public readonly static Guid SingletonId = Guid.Empty;
    private Guid? _sessionId;
    private SessionType? _sessionType;
    public ActiveSession()
    {

    }
    public ActiveSession(Guid id)
    {
        Raise(new ActiveSessionTrackingStarted(id));
    }
    private void When(ActiveSessionTrackingStarted started)
    {
        Id = started.TrackingId;
    }
    public void ActivateSession(Guid sessionId, SessionType sessionType)
    {
        if (_sessionId.HasValue && _sessionType.HasValue)
        {
            Raise(new SessionDeactivated(_sessionId.Value, _sessionType.Value));
        }

        Raise(new SessionActivated(sessionId, sessionType));
    }

    private void When(SessionActivated activated)
    {
        _sessionId = activated.SessionId;
        _sessionType = activated.SessionType;
    }

    public void DeactivateSession(Guid sessionId)
    {


        if (_sessionId != sessionId)
        {
            throw new InvalidOperationException($"Cannot deactivate session {sessionId} because it is not the active session.");
        }

        if (_sessionId.HasValue && _sessionType.HasValue)
        {
            Raise(new SessionDeactivated(_sessionId.Value, _sessionType.Value));
        }
    }
    private void When(SessionDeactivated _)
    {
        _sessionId = null;
        _sessionType = null;
    }
}
