using EventDbLite.Aggregates;

namespace NaeTime.Command.Aggregates;
public class HardwareDetection : AggregateRoot<Guid>
{
    private enum SessionType
    {
        OpenPractice,
    }

    private Guid? _sessionId;
    private SessionType? _sessionType;
    private Guid _timerId;
    private byte _lane;
    private ulong? _hardwareTime;
    private long _softwareTime;
    private DateTime _utcTime;

    public HardwareDetection()
    {

    }
    public HardwareDetection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Raise(new Events.HardwareDetectionOccured(detectionId, timerId, lane, hardwareTime, softwareTime, utcTime));
    }

    private void When(Events.HardwareDetectionOccured occured)
    {
        Id = occured.DetectionId;
        _timerId = occured.TimerId;
        _lane = occured.Lane;
        _hardwareTime = occured.HardwareTime;
        _softwareTime = occured.SoftwareTime;
        _utcTime = occured.UtcTime;
    }
    public void AssignDetectionToOpenPracticeSession(Guid detectionId, Guid sessionId)
    {
        if (_sessionId.HasValue && _sessionType.HasValue)
        {
            throw new InvalidOperationException($"Cannot assign detection {detectionId} to session {sessionId} because it is already assigned to session {_sessionId} of type {_sessionType}.");
        }

        Raise(new Events.HardwareDetectionAssignedToOpenPracticeSession(detectionId, sessionId, _timerId, _lane, _hardwareTime, _softwareTime, _utcTime));
    }
    public void When(Events.HardwareDetectionAssignedToOpenPracticeSession assigned)
    {
        _sessionId = assigned.SessionId;
        _sessionType = SessionType.OpenPractice;
    }
}
