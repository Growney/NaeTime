using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class Track : AggregateRoot
{
    private Dictionary<byte, Guid> _detectors = new();
    private Dictionary<Guid, byte> _detectorPositions = new();

    private Dictionary<Guid, long> _pilotMaximumTimes = new();
    private Dictionary<Guid, long> _pilotMinimumTimes = new();

    public Track()
    {

    }

    public Track(Guid id, Guid[] detectors, string name)
    {
        Raise(new TrackDesigned(id, name, detectors));
        for (byte i = 0; i < detectors.Length; i++)
        {
            Raise(new TrackDetectorAdded(id, detectors[i], i));
        }
        Raise(new TrackRenamed(id, name));
    }

    private void When(TrackDesigned trackDesigned)
    {
        Id = trackDesigned.TrackId;
    }
    private void When(TrackDetectorAdded trackDetectorAdded)
    {
        _detectors[trackDetectorAdded.OrdinalPosition] = trackDetectorAdded.DetectorId;
        _detectorPositions[trackDetectorAdded.DetectorId] = trackDetectorAdded.OrdinalPosition;
    }
    private void When(TrackDetectorRemoved trackDetectorRemoved)
    {
        _detectors.Remove(_detectors.FirstOrDefault(x => x.Value == trackDetectorRemoved.DetectorId).Key);
        _detectorPositions.Remove(trackDetectorRemoved.DetectorId);
    }
    private void When(TrackDetectorMoved trackDetectorMoved)
    {
        _detectors[trackDetectorMoved.OrdinalPosition] = trackDetectorMoved.DetectorId;
        _detectorPositions[trackDetectorMoved.DetectorId] = trackDetectorMoved.OrdinalPosition;
    }

    public void Rename(string? name)
    {
        Raise(new TrackRenamed(Id, name));
    }

    public void ReorderDetectors(Guid[] detectors)
    {
        List<object> newEvents = new();
        for (byte i = 0; i < detectors.Length; i++)
        {
            if (_detectors.TryGetValue(i, out var detectorId))
            {
                if (detectorId != detectors[i])
                {
                    newEvents.Add(new TrackDetectorMoved(Id, detectorId, i));
                }
            }
            else
            {
                newEvents.Add(new TrackDetectorAdded(Id, detectors[i], i));
            }
        }
        foreach (var detector in _detectors)
        {
            if (!detectors.Contains(detector.Value))
            {
                newEvents.Add(new TrackDetectorRemoved(Id, detector.Value));
            }
        }

        foreach (var newEvent in newEvents)
        {
            Raise(newEvent);
        }
    }

    public void SetMaximumLapTime(long maximumMilliseconds)
    {
        Raise(new TrackMaximumLapTimeConfigured(Id, maximumMilliseconds));
    }
    public void SetMinimumLapTime(long minimumMilliseconds)
    {
        Raise(new TrackMinimumLapTimeConfigured(Id, minimumMilliseconds));
    }
    public void ResetPilotMaximumTimeLapTime(Guid pilotId)
    {
        if (!_pilotMaximumTimes.ContainsKey(pilotId))
        {
            return;
        }

        Raise(new TrackPilotMaximumLapTimeReset(Id, pilotId));
    }
    public void ResetPilotMinimumTimeLapTime(Guid pilotId)
    {
        if (!_pilotMinimumTimes.ContainsKey(pilotId))
        {
            return;
        }

        Raise(new TrackPilotMinimumLapTimeReset(Id, pilotId));
    }
    public void SetPilotMaximumLapTime(Guid pilotId, long maximumMilliseconds)
    {
        Raise(new TrackPilotMaximumLapTimeConfigured(Id, pilotId, maximumMilliseconds));
    }
    public void SetPilotMinimumLapTime(Guid pilotId, long minimumMilliseconds)
    {
        Raise(new TrackPilotMinimumLapTimeConfigured(Id, pilotId, minimumMilliseconds));
    }
    private void When(TrackPilotMaximumLapTimeConfigured configured)
    {
        _pilotMaximumTimes[configured.PilotId] = configured.MaximumMilliseconds;
    }
    private void When(TrackPilotMinimumLapTimeConfigured configured)
    {
        _pilotMinimumTimes[configured.PilotId] = configured.MinimumMilliseconds;
    }
    private void When(TrackPilotMaximumLapTimeReset reset)
    {
        _pilotMaximumTimes.Remove(reset.PilotId);
    }
    private void When(TrackPilotMinimumLapTimeReset reset)
    {
        _pilotMinimumTimes.Remove(reset.PilotId);
    }
}

