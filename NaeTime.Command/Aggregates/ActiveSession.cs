using EventDbLite.Aggregates;
using NaeTime.Events.Domain;

namespace NaeTime.Command.Aggregates;
public class ActiveSession : AggregateRoot
{
    private enum SessionType
    {
        OpenPractice,
    }

    private Guid? _sessionId;
    private SessionType? _sessionType;

    public void ActivateOpenPracticeSession(Guid sessionId)
    {
        if (_sessionId == sessionId)
        {
            return;
        }

        if (_sessionId.HasValue && _sessionType.HasValue)
        {
            switch (_sessionType)
            {
                case SessionType.OpenPractice:
                    Raise(new OpenPracticeSessionDeactivated(_sessionId.Value));
                    break;
                default:
                    break;
            }
        }

        Raise(new OpenPracticeSessionActivated(sessionId));
    }

    private void When(OpenPracticeSessionActivated activated)
    {
        _sessionId = activated.SessionId;
        _sessionType = SessionType.OpenPractice;
    }

    public void DeactivateSession(Guid sessionId)
    {
        if (_sessionId != sessionId)
        {
            throw new InvalidOperationException($"Cannot deactivate session {sessionId} because it is not the active session.");
        }

        if (_sessionId.HasValue && _sessionType.HasValue)
        {
            Raise(new OpenPracticeSessionDeactivated(_sessionId.Value));
        }
    }
    private void When(OpenPracticeSessionDeactivated _)
    {
        _sessionId = null;
        _sessionType = null;
    }
}
