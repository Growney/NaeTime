using EventDbLite.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class OpenPracticePilotTiming : AggregateRoot<string>
{
    private class Detection
    {
        public Guid SessionId { get; init; }
        public Guid DetectionId { get; init; }
        public ulong? HardwareTime { get; init; }
        public long SoftwareTime { get; init; }
        public DateTime UtcTime { get; init; }
        public bool IsValid { get; set; } = true;
    }

    private readonly Dictionary<Guid, Dictionary<Guid, List<Detection>>> _pilotDetections = new();

    public OpenPracticePilotTiming(Guid sessionId, Guid pilotId)
    {
        Raise(new PilotOpenPracticeSessionTimingStarted(pilotId, sessionId));
    }
    private void When(PilotOpenPracticeSessionTimingStarted e)
    {
        Id = $"{e.SessionId:N}-{e.PilotId:N}";
    }

    public void AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Raise(new OpenPracticeDetectionAddedToPilot(pilotId, sessionId, detectionId, hardwareTime, softwareTime, utcTime));
    }
    private void When(OpenPracticeDetectionAddedToPilot e)
    {
        if(!_pilotDetections.TryGetValue(e.SessionId, out Dictionary<Guid, List<Detection>>? sessionPilots))
        {
            sessionPilots = new Dictionary<Guid, List<Detection>>();
            _pilotDetections[e.SessionId] = sessionPilots;
        }

        if(!sessionPilots.TryGetValue(e.PilotId, out List<Detection>? detections))
        {
            detections = new List<Detection>();
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
    public void RemoveDetectionFromPilot(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        Raise(new OpenPracticeDetectionRemovedFromPilot(pilotId, sessionId, detectionId));
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
    public void MarkPilotDetectionAsValid(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        Raise(new OpenPracticeDetectionValidated(pilotId, sessionId, detectionId));
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

    public void MarkPilotDetectionAsInvalid(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        Raise(new OpenPracticeDetectionInvalidated(pilotId, sessionId, detectionId));
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
