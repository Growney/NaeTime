using EventDbLite.Aggregates;

namespace NaeTime.Command.Aggregates;
public class Detection : AggregateRoot<Guid>
{
    private enum SessionType
    {
        OpenPractice,
    }

    private Guid? _sessionId;
    private SessionType? _sessionType;

    public Detection(Guid DetectionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime)
    {
        Raise(new Events.HardwareDetectionOccured(DetectionId, TimerId, Lane, HardwareTime, SoftwareTime, UtcTime));
    }

    public Detection()
    {

    }
    public Detection(Guid DetectionId, byte Lane, long SoftwareTime, DateTime UtcTime)
    {
        Raise(new Events.DetectionTriggered(DetectionId, Lane, SoftwareTime, UtcTime));
    }

    public void AssignToOpenPracticeSession(Guid DetectionId, Guid SessionId)
    {
        if (_sessionId.HasValue && _sessionType.HasValue)
        {
            Raise(new Events.DetectionUnassignedOpenPracticeFromSession(DetectionId, _sessionId.Value));
        }

        Raise(new Events.DetectionAssignedToOpenPracticeSession(DetectionId, SessionId));
    }
    public void When(Events.DetectionAssignedToOpenPracticeSession assigned)
    {
        _sessionId = assigned.SessionId;
        _sessionType = SessionType.OpenPractice;
    }
    public void UnassignFromSession(Guid DetectionId, Guid SessionId)
    {
        if (_sessionId != SessionId || !_sessionType.HasValue)
        {
            throw new InvalidOperationException($"Cannot unassign detection {DetectionId} from session {SessionId} because it is not assigned to that session.");
        }

        switch (_sessionType)
        {
            case SessionType.OpenPractice:
                Raise(new Events.DetectionUnassignedOpenPracticeFromSession(DetectionId, SessionId));
                break;
            default:
                break;
        }
    }
    public void When(Events.DetectionUnassignedOpenPracticeFromSession _)
    {
        _sessionId = null;
    }
}
