using EventDbLite.Projections;
using NaeTime.Events;

namespace NaeTime.Query.Projections;
public class OpenPracticePilotTimings : Projection
{
    private readonly List<Query.Abstractions.Models.OpenPracticePilotDetection> _pilotDetections = new();

    private Query.Abstractions.Models.OpenPracticeSessionPilotLap? _activeLap;

    public void When(OpenPracticePilotDetectionOccured detection)
    {
        _pilotDetections.Add(new Abstractions.Models.OpenPracticePilotDetection(detection.DetectionId, detection.SessionId, detection.PilotId, detection.TrackTimerOrdinal, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
    }
    public void When(OpenPracticePilotDetectionTriggered detection)
    {
        _pilotDetections.Add(new Abstractions.Models.OpenPracticePilotDetection(detection.DetectionId, detection.SessionId, detection.PilotId, detection.OrdinalPosition, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
    }

    public void When(OpenPracticeLapStarted started)
    {
        _activeLap = new Query.Abstractions.Models.OpenPracticeSessionPilotLap(started.LapId, started.SessionId, started.LapId, new Abstractions.Models.OpenPracticeDetection(started. )
    }

}
