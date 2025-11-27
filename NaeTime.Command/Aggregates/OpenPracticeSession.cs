using EventDbLite.Aggregates;
using NaeTime.Events;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
namespace NaeTime.Command.Aggregates;
public class OpenPracticeSession : AggregateRoot<Guid>
{
    private class LaneInfo(Guid? pilotId, bool isEnabled, OpenPracticeSession.LaneFrequency frequency)
    {
        public Guid? PilotId { get; set; } = pilotId;
        public bool IsEnabled { get; set; } = isEnabled;
        public LaneFrequency Frequency { get; set; } = frequency ?? throw new ArgumentNullException(nameof(frequency));
    }
    private class LaneFrequency(byte? bandId, int frequencyInMHz)
    {
        public byte? BandId { get; set; } = bandId;
        public int FrequencyInMHz { get; set; } = frequencyInMHz;
    }
    private class Detection
    {
        public Guid Id { get; init; }
        public Guid PilotId { get; init; }
        public Guid? TimerId { get; init; }
        public byte Lane { get; init; }
        public byte TrackTimerOrdinal { get; init; }
        public byte TrackTimerTotal { get; init; }
        public ulong? HardwareTime { get; init; }
        public long SoftwareTime { get; init; }
        public DateTime UtcTime { get; init; }
        public bool IsValid { get; set; } = true;
        public Guid? PackEndBeforeDetection { get; set; }
        public Guid? PackEndAfterDetection { get; set; }
    }
    private readonly Dictionary<Guid,Detection> _detections = [];
    private readonly Dictionary<Guid, Detection> _pilotLastDetection = [];
    private readonly Dictionary<byte, LaneInfo> _lanes = [];
    private readonly Dictionary<Guid, Detection> _packEndDetections = [];
    private Guid[] _trackDetectors = [];
    private Guid _trackId;
    private TimeSpan? _redetectionDelay;
    public OpenPracticeSession()
    {

    }
    public OpenPracticeSession(Guid id, Guid trackId, Guid[] trackDetectors, string name)
    {
        Raise(new OpenPracticeSessionScheduled(id, name, trackId, trackDetectors));
    }

    public void Rename(string name)
    {
        Raise(new OpenPracticeSessionRenamed(Id, name));
    }

    public void ConfigureDefaultLanes(byte laneCount)
    {
        for (byte i = 0; i < laneCount; i++)
        {
            Raise(new OpenPracticeSessionLaneEnabled(Id, i, _trackDetectors));

            int frequencyIndex = i % Hardware.Frequency.Band.R.Frequencies.Count();

            Raise(new OpenPracticeSessionLaneVideoFrequencyTuned(Id, i, Hardware.Frequency.Band.R.Id, Hardware.Frequency.Band.R.Frequencies.ElementAt(frequencyIndex).FrequencyInMhz, _trackDetectors));
        }
    }

    private LaneInfo GetLaneInfo(byte lane)
    {
        if (!_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            laneInfo = new LaneInfo(null, false, new LaneFrequency(null, 0));
            _lanes.Add(lane, laneInfo);
        }
        return laneInfo;
    }

    private void When(OpenPracticeSessionScheduled scheduledEvent)
    {
        Id = scheduledEvent.SessionId;
        _trackId = scheduledEvent.TrackId;
        _trackDetectors = scheduledEvent.TrackDetectors;
    }
    private void When(OpenPracticeSessionLaneEnabled laneEnabled)
    {
        LaneInfo laneInfo = GetLaneInfo(laneEnabled.Lane);
        laneInfo.IsEnabled = true;
    }
    private void When(OpenPracticeSessionLaneDisabled laneDisabled)
    {
        LaneInfo laneInfo = GetLaneInfo(laneDisabled.Lane);
        laneInfo.IsEnabled = false;
    }
    private void When(OpenPracticeSessionLaneVideoFrequencyTuned laneVideoFrequencyTuned)
    {
        LaneInfo laneInfo = GetLaneInfo(laneVideoFrequencyTuned.Lane);
        laneInfo.Frequency.BandId = laneVideoFrequencyTuned.BandId;
        laneInfo.Frequency.FrequencyInMHz = laneVideoFrequencyTuned.FrequencyInMHz;
    }
    private void When(OpenPracticeSessionLanePilotSet lanePilotSet)
    {
        LaneInfo laneInfo = GetLaneInfo(lanePilotSet.Lane);
        laneInfo.PilotId = lanePilotSet.PilotId;
    }
    private void When(OpenPracticeSessionLanePilotReset lanePilotReset)
    {
        LaneInfo laneInfo = GetLaneInfo(lanePilotReset.Lane);
        laneInfo.PilotId = null;
    }
    private void When(OpenPracticePilotDetectionTriggered triggered)
    {
        Detection detection = new()
        {
            Id = triggered.DetectionId,
            PilotId = triggered.PilotId,
            TimerId = null,
            Lane = triggered.Lane,
            TrackTimerOrdinal = triggered.OrdinalPosition,
            TrackTimerTotal = triggered.TrackDetectorCount,
            HardwareTime = triggered.HardwareTime,
            SoftwareTime = triggered.SoftwareTime,
            UtcTime = triggered.UtcTime,
        };

        _pilotLastDetection[triggered.PilotId] = detection;
        _detections[triggered.DetectionId] = detection;
    }

    private void When(OpenPracticePilotDetectionOccured occured)
    {
        Detection detection = new ()
        {
            Id = occured.DetectionId,
            TimerId = occured.TimerId,
            TrackTimerOrdinal = occured.TrackTimerOrdinal,
            TrackTimerTotal = occured.TrackTimerTotal,
            HardwareTime = occured.HardwareTime,
            SoftwareTime = occured.SoftwareTime,
            UtcTime = occured.UtcTime,
            PilotId = occured.PilotId,
        };

        _pilotLastDetection[occured.PilotId] = detection;
        _detections[occured.DetectionId] = detection;
    }

    private void When(OpenPracticePilotDetectionInvalidated invalidated)
    {
        _detections[invalidated.DetectionId].IsValid = false;
    }

    private void When(OpenPracticePilotDetectionValidated validated)
    {
        _detections[validated.DetectionId].IsValid = true;
    }

    private void When(OpenPracticePilotPackEndInsertedAfterDetection inserted)
    {
        _detections[inserted.DetectionId].PackEndAfterDetection = inserted.PackEndId;
        _packEndDetections[inserted.PackEndId] = _detections[inserted.DetectionId];
    }

    private void When(OpenPracticePilotPackEndInsertedBeforeDetection inserted)
    {
        _detections[inserted.DetectionId].PackEndBeforeDetection = inserted.PackEndId;
        _packEndDetections[inserted.PackEndId] = _detections[inserted.DetectionId];
    }
    private void When(OpenPracticePilotPackEndRemoved removed)
    {
        Detection detection = _detections[removed.DetectionId];
        if (detection.PackEndBeforeDetection == removed.PackEndId)
        {
            detection.PackEndBeforeDetection = null;
        }
        if (detection.PackEndAfterDetection == removed.PackEndId)
        {
            detection.PackEndAfterDetection = null;
        }
        _packEndDetections.Remove(removed.PackEndId);
    }
    public void InsertPilotPackEndBeforeDetection(Guid detectionId)
    {
        _detections.TryGetValue(detectionId, out Detection? detection);
        if (detection is null)
        {
            throw new ValidationException($"Detection {detectionId} does not exist.");
        }
        Raise(new OpenPracticePilotPackEndInsertedBeforeDetection(detectionId, Guid.NewGuid(), Id, _trackId, detection.PilotId));
        Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, detection.PilotId));
    }
    public void InsertPilotPackEndAfterDetection(Guid detectionId)
    {
        _detections.TryGetValue(detectionId, out Detection? detection);
        if (detection is null)
        {
            throw new ValidationException($"Detection {detectionId} does not exist.");
        }
        Raise(new OpenPracticePilotPackEndInsertedAfterDetection(detectionId,Guid.NewGuid(),Id,_trackId,detection.PilotId));
        Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, detection.PilotId));
    }
    public void RemovePilotPackEnd(Guid packEndId)
    {
        _packEndDetections.TryGetValue(packEndId, out Detection? detection);
        if (detection is null)
        {
            throw new ValidationException($"Detection does not exist. For pack end");
        }
        if (detection.PackEndBeforeDetection != packEndId && detection.PackEndAfterDetection != packEndId)
        {
            throw new ValidationException($"Pack end {packEndId} is not associated with detection {detection.Id}.");
        }
        Raise(new OpenPracticePilotPackEndRemoved(packEndId,detection.Id,Id,_trackId, detection.PilotId));
        Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, detection.PilotId));
    }
    public void InvalidatePilotDetection(Guid detectionId)
    {
        _detections.TryGetValue(detectionId, out Detection? detection);
        
        if (detection is null)
        {
            throw new ValidationException($"Detection {detectionId} does not exist.");
        }

        Raise(new OpenPracticePilotDetectionInvalidated(detectionId, Id, _trackId, detection.PilotId));
        Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, detection.PilotId));
    }
    public void InvalidateAllPilotDetections(Guid pilotId)
    {
        var detectionsToInvalidate = _detections.Values.Where(d => d.PilotId == pilotId && d.IsValid).ToList();
        foreach (var detection in detectionsToInvalidate)
        {
            Raise(new OpenPracticePilotDetectionInvalidated(detection.Id, Id, _trackId, pilotId));
        }
        if (detectionsToInvalidate.Any())
        {
            Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, pilotId));
        }
    }
    public void InvalidatePilotDetectionsBeforeDetection(Guid detectionId)
    {
        _detections.TryGetValue(detectionId, out Detection? detection);
        if (detection is null)
        {
            throw new ValidationException($"Detection {detectionId} does not exist.");
        }
        List<Detection> detectionsToInvalidate = _detections.Values.Where(d => d.PilotId == detection.PilotId && d.UtcTime < detection.UtcTime && d.IsValid).ToList();
        foreach (var det in detectionsToInvalidate)
        {
            Raise(new OpenPracticePilotDetectionInvalidated(det.Id, Id, _trackId, detection.PilotId));
        }
        if (detectionsToInvalidate.Any())
        {
            Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, detection.PilotId));
        }
    }
    public void ValidatePilotDetection(Guid detectionId)
    {
        _detections.TryGetValue(detectionId, out Detection? detection);

        if (detection is null)
        {
            throw new ValidationException($"Detection {detectionId} does not exist.");
        }

        Raise(new OpenPracticePilotDetectionValidated(detectionId, Id, _trackId, _detections[detectionId].PilotId));
        Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, detection.PilotId));
    }
    public void SetLanePilot(byte lane, Guid pilotId)
    {
        foreach (LaneInfo laneInfo in _lanes.Values)
        {
            if (laneInfo.PilotId == pilotId)
            {
                throw new InvalidOperationException($"Pilot {pilotId} is already assigned to a lane.");
            }
        }

        Raise(new OpenPracticeSessionLanePilotSet(Id, lane, pilotId));
    }
    public void ResetLanePilot(byte lane)
    {
        if (_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            Raise(new OpenPracticeSessionLanePilotReset(Id, lane));
        }
    }
    public void TuneLaneVideoFrequency(byte lane, byte? bandId, int frequencyInMHz)
    {
        Raise(new OpenPracticeSessionLaneVideoFrequencyTuned(Id, lane, bandId, frequencyInMHz, _trackDetectors));
    }
    public void EnableLane(byte lane)
    {
        if (_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            if (!laneInfo.IsEnabled)
            {
                Raise(new OpenPracticeSessionLaneEnabled(Id, lane, _trackDetectors));
            }
        }
        else
        {
            Raise(new OpenPracticeSessionLaneEnabled(Id, lane, _trackDetectors));
        }
    }
    public void DisableLane(byte lane)
    {
        if (_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            if (laneInfo.IsEnabled)
            {
                Raise(new OpenPracticeSessionLaneDisabled(Id, lane, _trackDetectors));
            }
        }
        else
        {
            Raise(new OpenPracticeSessionLaneDisabled(Id, lane, _trackDetectors));
        }
    }
    public OpenPracticeSession Clone(Guid newId, string newName)
    {
        OpenPracticeSession clone = new(newId, _trackId, _trackDetectors, newName);
        Clone(clone);
        CloneLanes(clone);

        return clone;
    }
    public OpenPracticeSession Clone(Guid newId, Guid newTrackId, string newName)
    {
        OpenPracticeSession clone = new(newId, newTrackId, _trackDetectors, newName);
        Clone(clone);
        CloneLanes(clone);

        return clone;
    }
    public void AssignHardwareDetection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Raise(new OpenPracticeHardwareDetectionOccured(detectionId, Id, timerId, _trackId, lane, hardwareTime, softwareTime, utcTime));

        LaneInfo laneInfo = GetLaneInfo(lane);
        if (!laneInfo.IsEnabled)
        {
            Raise(new OpenPracticeHardwareDetectionIgnoredOnDisabledLane(detectionId, Id, timerId, _trackId, lane, hardwareTime, softwareTime, utcTime));
            return;
        }
        if (laneInfo.PilotId is null)
        {
            Raise(new OpenPracticeHardwareDetectionIgnoredOnUnassignedLane(detectionId, Id, timerId, _trackId, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        Guid pilotId = laneInfo.PilotId.Value;

        _pilotLastDetection.TryGetValue(pilotId, out Detection? mostRecentDetection);

        int trackTimerOrdinal = Array.FindIndex(_trackDetectors, x => x == timerId);

        if (trackTimerOrdinal < 0 || trackTimerOrdinal >= _trackDetectors.Length)
        {
            Raise(new OpenPracticePilotDetectionOccuredOnInvalidTimer(detectionId, Id, _trackId, pilotId, timerId, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        if (mostRecentDetection is not null && trackTimerOrdinal == mostRecentDetection.TrackTimerOrdinal && _redetectionDelay.HasValue)
        {
            TimeSpan timeSinceLastDetection = CalculateDuration(mostRecentDetection.TimerId, mostRecentDetection.HardwareTime, mostRecentDetection.SoftwareTime, mostRecentDetection.UtcTime,
                timerId, hardwareTime, softwareTime, utcTime);
            if (timeSinceLastDetection < _redetectionDelay.Value)
            {
                Raise(new OpenPracticePilotDetectionOccuredToCloseToPreviousDetection(detectionId, Id, _trackId, pilotId, timerId, lane, hardwareTime, softwareTime, utcTime,
                    mostRecentDetection.Id, mostRecentDetection.TimerId, mostRecentDetection.Lane, mostRecentDetection.HardwareTime, mostRecentDetection.SoftwareTime, mostRecentDetection.UtcTime));
                return;
            }
        }

        if (mostRecentDetection is not null && utcTime < mostRecentDetection.UtcTime)
        {
            Raise(new OpenPracticePilotDetectionOccuredOutOfOrder(detectionId, Id, _trackId, pilotId, timerId, (byte)trackTimerOrdinal, (byte)_trackDetectors.Length, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        Raise(new OpenPracticePilotDetectionOccured(detectionId, Id, _trackId, pilotId, timerId, (byte)trackTimerOrdinal, (byte)_trackDetectors.Length, lane, hardwareTime, softwareTime, utcTime));
        Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, pilotId));
    }
    public void TriggerDetection(Guid detectionId, byte lane, byte ordinalPosition, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Guid? pilotId = _lanes.TryGetValue(lane, out LaneInfo? laneInfo) && laneInfo.PilotId.HasValue
            ? laneInfo.PilotId : null;

        if (pilotId is null)
        {
            Raise(new OpenPracticeDetectionTriggerIgnoredOnUnassignedLane(detectionId, Id, _trackId, ordinalPosition, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        _pilotLastDetection.TryGetValue(pilotId.Value, out Detection? mostRecentDetection);

        if (mostRecentDetection is not null && ordinalPosition == mostRecentDetection.TrackTimerOrdinal && _redetectionDelay.HasValue)
        {
            TimeSpan timeSinceLastDetection = CalculateDuration(mostRecentDetection.TimerId, mostRecentDetection.HardwareTime, mostRecentDetection.SoftwareTime, mostRecentDetection.UtcTime,
                null, hardwareTime, softwareTime, utcTime);
            if (timeSinceLastDetection < _redetectionDelay.Value)
            {
                Raise(new OpenPracticePilotDetectionTriggeredToCloseToPreviousDetection(detectionId, Id, _trackId, pilotId.Value, lane, hardwareTime, softwareTime, utcTime,
                    mostRecentDetection.Id, mostRecentDetection.TimerId, mostRecentDetection.Lane, mostRecentDetection.HardwareTime, mostRecentDetection.SoftwareTime, mostRecentDetection.UtcTime));
                return;
            }
        }


        if (mostRecentDetection is not null && utcTime < mostRecentDetection.UtcTime)
        {
            Raise(new OpenPracticePilotDetectionTriggeredOutOfOrder(detectionId, Id, _trackId, pilotId.Value, ordinalPosition, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime));
            return;
        }

        Raise(new OpenPracticePilotDetectionTriggered(detectionId, Id, _trackId, pilotId.Value, lane, ordinalPosition, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime));
        Raise(new OpenPracticePilotTimingChangeOccured(Id, _trackId, pilotId.Value));
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
    private void CloneLanes(OpenPracticeSession clone)
    {
        foreach (KeyValuePair<byte, LaneInfo> laneInfo in _lanes)
        {
            if (laneInfo.Value.IsEnabled)
            {
                clone.EnableLane(laneInfo.Key);
            }
            else
            {
                clone.DisableLane(laneInfo.Key);
            }
            if (laneInfo.Value.PilotId.HasValue)
            {
                clone.SetLanePilot(laneInfo.Key, laneInfo.Value.PilotId.Value);
            }
            clone.TuneLaneVideoFrequency(laneInfo.Key, laneInfo.Value.Frequency.BandId, laneInfo.Value.Frequency.FrequencyInMHz);
        }
    }
}
