using EventDbLite.Aggregates;

namespace NaeTime.Command.Aggregates;
public class Detection : AggregateRoot
{
    private Guid? _sessionId;

    public Detection(Guid DetectionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime)
    {
        Raise(new Events.HardwareDetectionOccured(DetectionId, TimerId, Lane, HardwareTime, SoftwareTime, UtcTime));
    }

    public Detection()
    {

    }
    public void When(Events.HardwareDetectionOccured occured)
    {
        Id = occured.DetectionId;
    }
    public Detection(Guid DetectionId, byte Lane, long SoftwareTime, DateTime UtcTime)
    {
        Raise(new Events.DetectionTriggered(DetectionId, Lane, SoftwareTime, UtcTime));
    }
    public void When(Events.DetectionTriggered triggered)
    {
        Id = triggered.DetectionId;
    }

    public void AssignToSession(Guid DetectionId, Guid SessionId)
    {
        if (_sessionId.HasValue)
        {
            Raise(new Events.DetectionUnassignedFromSession(DetectionId, _sessionId.Value));
        }

        Raise(new Events.DetectionAssignedToSession(DetectionId, SessionId));
    }
    public void When(Events.DetectionAssignedToSession assigned)
    {
        _sessionId = assigned.SessionId;
    }
    public void UnassignFromSession(Guid DetectionId, Guid SessionId)
    {
        if (_sessionId != SessionId)
        {
            throw new InvalidOperationException($"Cannot unassign detection {DetectionId} from session {SessionId} because it is not assigned to that session.");
        }

        Raise(new Events.DetectionUnassignedFromSession(DetectionId, SessionId));
    }
    public void When(Events.DetectionUnassignedFromSession _)
    {
        _sessionId = null;
    }
}
