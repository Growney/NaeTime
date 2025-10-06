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
    private byte _lane;
    private ulong? _hardwareTime;
    private long _softwareTime;
    private DateTime _utcTime;

    public Detection()
    {

    }
    public Detection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Raise(new Events.HardwareDetectionOccured(detectionId, timerId, lane, hardwareTime, softwareTime, utcTime));
    }

    private void When(Events.HardwareDetectionOccured occured)
    {
        Id = occured.DetectionId;
        _lane = occured.Lane;
        _hardwareTime = occured.HardwareTime;
        _softwareTime = occured.SoftwareTime;
        _utcTime = occured.UtcTime;
    }
    public Detection(Guid detectionId, byte lane, long softwareTime, DateTime utcTime)
    {
        Raise(new Events.DetectionTriggered(detectionId, lane, softwareTime, utcTime));
    }
    private void When(Events.DetectionTriggered triggered)
    {
        Id = triggered.DetectionId;
        _lane = triggered.Lane;
        _softwareTime = triggered.SoftwareTime;
        _utcTime = triggered.UtcTime;
    }
    public void MarkDetectionWithinOpenPracticeBounds(Guid detectionId, Guid sessionId)
    {
        if (_sessionId.HasValue && _sessionType.HasValue)
        {
            throw new InvalidOperationException($"Cannot assign detection {detectionId} to session {sessionId} because it is already assigned to session {_sessionId} of type {_sessionType}.");
        }

        Raise(new Events.DetectionOccuredWithinOpenPracticeBounds(detectionId, sessionId, _lane, _hardwareTime, _softwareTime, _utcTime));
    }
    public void When(Events.DetectionOccuredWithinOpenPracticeBounds assigned)
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
