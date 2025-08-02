using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class ActiveSession : AggregateRoot
{
    private enum SessionType
    {
        OpenPractice,
    }
    private Guid? _sessionId;
    private SessionType? _sessionType;

    public ActiveSession()
    {

    }

    public void ActivateOpenPracticeSession(Guid sessionId)
    {
        if (_sessionId.HasValue)
        {
            switch (_sessionType)
            {
                case SessionType.OpenPractice:
                    Raise(new OpenPracticeSessionDeactivated(_sessionId.Value));
                    break;
                default:
                    throw new NotImplementedException($"Session type {_sessionType} is not implemented.");
            }
        }

        Raise(new OpenPracticeSessionActivated(sessionId));
    }

    private void When(OpenPracticeSessionActivated activated)
    {
        _sessionId = activated.SessionId;
        _sessionType = SessionType.OpenPractice;
    }

    public void DeactivateOpenPracticeSession(Guid sessionId)
    {
        if (_sessionId != sessionId)
        {
            throw new InvalidOperationException("Cannot deactivate a session that is not active.");
        }
        Raise(new OpenPracticeSessionDeactivated(sessionId));
    }
    private void When(OpenPracticeSessionDeactivated _)
    {
        _sessionId = null;
        _sessionType = null;
    }
}
