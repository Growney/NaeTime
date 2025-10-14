using EventDbLite.Aggregates;
using NaeTime.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NaeTime.Command.Aggregates.OpenPracticePilotTiming;

namespace NaeTime.Command.Aggregates;
public class OpenPracticePilotTiming : AggregateRoot<OpenPracticeTimingId>
{
    public class OpenPracticeTimingId
    {
        public Guid PilotId { get; init; }
        public Guid SessionId { get; init; }

        public override string ToString() => $"{SessionId:N}-{PilotId:N}";
    }
    private class Detection
    {
        public Guid SessionId { get; init; }
        public Guid DetectionId { get; init; }
        public ulong? HardwareTime { get; init; }
        public long SoftwareTime { get; init; }
        public DateTime UtcTime { get; init; }
        public bool IsValid { get; set; } = true;
    }

    private readonly Dictionary<Guid, Dictionary<Guid, List<Detection>>> _pilotDetections = [];

    public OpenPracticePilotTiming()
    {

    }

    public OpenPracticePilotTiming(Guid sessionId, Guid pilotId)
    {
        Raise(new PilotOpenPracticeSessionTimingStarted(pilotId, sessionId));
    }
    private void When(PilotOpenPracticeSessionTimingStarted e)
    {
        Id = new OpenPracticeTimingId
        {
            PilotId = e.PilotId,
            SessionId = e.SessionId
        };
    }

    public void AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Raise(new OpenPracticeDetectionAddedToPilot(pilotId, sessionId, detectionId, hardwareTime, softwareTime, utcTime));
    }
    private void When(OpenPracticeDetectionAddedToPilot e)
    {
        if(!_pilotDetections.TryGetValue(e.SessionId, out Dictionary<Guid, List<Detection>>? sessionPilots))
        {
            sessionPilots = [];
            _pilotDetections[e.SessionId] = sessionPilots;
        }

        if(!sessionPilots.TryGetValue(e.PilotId, out List<Detection>? detections))
        {
            detections = [];
            sessionPilots[e.PilotId] = detections;
        }

        if(detections.Any(d => d.DetectionId == e.DetectionId))
        {
            // already added
            return;
        }

        detections.Add( new Detection
        {
            SessionId = e.SessionId,
            DetectionId = e.DetectionId,
            HardwareTime = e.HardwareTime,
            SoftwareTime = e.SoftwareTime,
            UtcTime = e.UtcTime,
            IsValid = true
        });
    }
    public void RemoveDetectionFromPilot(Guid detectionId)
    {
        ArgumentNullException.ThrowIfNull(Id, nameof(Id));

        Raise(new OpenPracticeDetectionRemovedFromPilot(Id.PilotId, Id.SessionId, detectionId));
    }

    private void When(OpenPracticeDetectionRemovedFromPilot e)
    {
        if (_pilotDetections.TryGetValue(e.SessionId, out Dictionary<Guid, List<Detection>>? sessionPilots))
        {
            if (sessionPilots.TryGetValue(e.PilotId, out List<Detection>? detections))
            {
                var detection = detections.FirstOrDefault(d => d.DetectionId == e.DetectionId);
                if (detection != null)
                {
                    detections.Remove(detection);
                }
            }
        }
    }
    public void MarkPilotDetectionAsValid(Guid detectionId)
    {
        ArgumentNullException.ThrowIfNull(Id, nameof(Id));

        Raise(new OpenPracticeDetectionValidated(Id.PilotId, Id.SessionId, detectionId));
    }

    private void When(OpenPracticeDetectionValidated e)
    {
        if (_pilotDetections.TryGetValue(e.SessionId, out Dictionary<Guid, List<Detection>>? sessionPilots))
        {
            if (sessionPilots.TryGetValue(e.PilotId, out List<Detection>? detections))
            {
                var detection = detections.FirstOrDefault(d => d.DetectionId == e.DetectionId);
                if (detection != null)
                {
                    detection.IsValid = true;
                }
            }
        }
    }

    public void MarkPilotDetectionAsInvalid(Guid detectionId)
    {
        ArgumentNullException.ThrowIfNull(Id, nameof(Id));

        Raise(new OpenPracticeDetectionInvalidated(Id.PilotId, Id.SessionId, detectionId));
    }

    private void When(OpenPracticeDetectionInvalidated e)
    {
        if (_pilotDetections.TryGetValue(e.SessionId, out Dictionary<Guid, List<Detection>>? sessionPilots))
        {
            if (sessionPilots.TryGetValue(e.PilotId, out List<Detection>? detections))
            {
                var detection = detections.FirstOrDefault(d => d.DetectionId == e.DetectionId);
                if (detection != null)
                {
                    detection.IsValid = false;
                }
            }
        }
    }
}
