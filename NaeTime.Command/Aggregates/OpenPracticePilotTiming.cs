using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class OpenPracticePilotTiming : AggregateRoot<OpenPracticePilotTiming.OpenPracticePilotTimingId>
{
    public class OpenPracticePilotTimingId
    {
        public Guid PilotId { get; init; }
        public Guid SessionId { get; init; }
        public Guid TrackId { get; init; }
        public override string ToString() => $"{SessionId:N}-{TrackId:N}-{PilotId:N}";
    }

    private class Detection
    {
        public Guid Id { get; init; }
        public Guid? TimerId { get; init; }
        public byte Lane { get; init; }
        public byte TrackTimerOrdinal { get; init; }
        public byte TrackTimerTotal { get; init; }
        public ulong? HardwareTime { get; init; }
        public long SoftwareTime { get; init; }
        public DateTime UtcTime { get; init; }

    }

    private Guid[] _trackDetectors = [];

    private Detection? _mostRecentDetection;

    private TimeSpan? _redetectionDelay;

    public OpenPracticePilotTiming()
    {

    }
    public OpenPracticePilotTiming(Guid pilotId, Guid sessionId, Guid trackId, Guid[] trackDetectors, long? redetectionDelayMilliseconds, long? maximumLapMilliseconds)
    {
        Raise(new OpenPracticePilotTimingStarted(pilotId, sessionId, trackId, trackDetectors, redetectionDelayMilliseconds, maximumLapMilliseconds));
    }
    private static TimeSpan CalculateDuration(Guid? startTimerId, ulong? startHardwareTime, long startSoftwareTime, DateTime startUtcTime,
        Guid? endTimerId, ulong? endHardwareTime, long endSoftwareTime, DateTime endUtcTime)
    {
        if (startTimerId.HasValue && endTimerId.HasValue && startTimerId == endTimerId && startHardwareTime.HasValue && endHardwareTime.HasValue)
        {
            if (endHardwareTime.Value >= startHardwareTime.Value)
            {
                return TimeSpan.FromMilliseconds(endHardwareTime.Value - startHardwareTime.Value);
            }
        }
        if (endSoftwareTime >= startSoftwareTime)
        {
            return TimeSpan.FromMilliseconds(endSoftwareTime - startSoftwareTime);
        }
        if (endUtcTime >= startUtcTime)
        {
            return endUtcTime - startUtcTime;
        }
        return CalculateDuration(endTimerId, endHardwareTime, endSoftwareTime, endUtcTime, startTimerId, startHardwareTime, startSoftwareTime, startUtcTime);
    }
    public void AddDetectionOccurance(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        ArgumentNullException.ThrowIfNull(Id);

        int trackTimerOrdinal = Array.FindIndex(_trackDetectors, x => x == timerId);

        if (trackTimerOrdinal < 0 || trackTimerOrdinal >= _trackDetectors.Length)
        {
            Raise(new OpenPracticePilotDetectionOccuredOnInvalidTimer(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        if (_mostRecentDetection is not null && trackTimerOrdinal == _mostRecentDetection.TrackTimerOrdinal && _redetectionDelay.HasValue)
        {
            TimeSpan timeSinceLastDetection = CalculateDuration(_mostRecentDetection.TimerId, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime,
                timerId, hardwareTime, softwareTime, utcTime);
            if (timeSinceLastDetection < _redetectionDelay.Value)
            {
                Raise(new OpenPracticePilotDetectionOccuredToCloseToPreviousDetection(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, lane, hardwareTime, softwareTime, utcTime,
                    _mostRecentDetection.Id, _mostRecentDetection.TimerId, _mostRecentDetection.Lane, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime));
                return;
            }
        }

        if (_mostRecentDetection is not null && utcTime < _mostRecentDetection.UtcTime)
        {
            Raise(new OpenPracticePilotDetectionOccuredOutOfOrder(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, (byte)trackTimerOrdinal, (byte)_trackDetectors.Length, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        Raise(new OpenPracticePilotDetectionOccured(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, (byte)trackTimerOrdinal, (byte)_trackDetectors.Length, lane, hardwareTime, softwareTime, utcTime));
    }
    public void TriggerDetection(Guid detectionId, byte lane, byte ordinalPosition, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        ArgumentNullException.ThrowIfNull(Id);

        if (_mostRecentDetection is not null && ordinalPosition == _mostRecentDetection.TrackTimerOrdinal && _redetectionDelay.HasValue)
        {
            TimeSpan timeSinceLastDetection = CalculateDuration(_mostRecentDetection.TimerId, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime,
                null, hardwareTime, softwareTime, utcTime);
            if (timeSinceLastDetection < _redetectionDelay.Value)
            {
                Raise(new OpenPracticePilotDetectionTriggeredToCloseToPreviousDetection(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, lane, hardwareTime, softwareTime, utcTime,
                    _mostRecentDetection.Id, _mostRecentDetection.TimerId, _mostRecentDetection.Lane, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime));
                return;
            }
        }


        if (_mostRecentDetection is not null && utcTime < _mostRecentDetection.UtcTime)
        {
            Raise(new OpenPracticePilotDetectionTriggeredOutOfOrder(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, ordinalPosition, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime));
            return;
        }

        Raise(new OpenPracticePilotDetectionTriggered(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, lane, ordinalPosition, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime));
    }

    private void When(OpenPracticePilotDetectionOccured occured)
    {
        _mostRecentDetection = new()
        {
            Id = occured.DetectionId,
            TimerId = occured.TimerId,
            TrackTimerOrdinal = occured.TrackTimerOrdinal,
            TrackTimerTotal = occured.TrackTimerTotal,
            HardwareTime = occured.HardwareTime,
            SoftwareTime = occured.SoftwareTime,
            UtcTime = occured.UtcTime
        };
    }
    private void When(OpenPracticePilotTimingStarted started)
    {
        Id = new OpenPracticePilotTimingId
        {
            PilotId = started.PilotId,
            SessionId = started.SessionId,
            TrackId = started.TrackId
        };
        _redetectionDelay = started.RedetectionDelayMilliseconds.HasValue ? TimeSpan.FromMilliseconds(started.RedetectionDelayMilliseconds.Value) : null;
        _trackDetectors = started.TrackDetectors;
    }
    private void When(OpenPracticePilotDetectionTriggered triggered)
    {
        _mostRecentDetection = new()
        {
            Id = triggered.DetectionId,
            TimerId = null,
            TrackTimerOrdinal = triggered.OrdinalPosition,
            TrackTimerTotal = triggered.TrackDetectorCount,
            HardwareTime = triggered.HardwareTime,
            SoftwareTime = triggered.SoftwareTime,
            UtcTime = triggered.UtcTime
        };
    }
}
