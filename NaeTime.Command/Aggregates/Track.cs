using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class Track : AggregateRoot<Guid>
{
    private Dictionary<byte, Guid> _detectors = [];
    private Dictionary<Guid, byte> _detectorPositions = [];

    private Dictionary<Guid, long> _pilotMaximumTimes = [];
    private Dictionary<Guid, long> _pilotMinimumTimes = [];

    public Track()
    {

    }

    public Track(Guid id, Guid[] detectors, string name)
    {
        Raise(new TrackDesigned(id, name, detectors));
    }

    private void When(TrackDesigned trackDesigned)
    {
        Id = trackDesigned.TrackId;
        _detectors.Clear();
        _detectorPositions.Clear();

        for (byte i = 0; i < trackDesigned.DetectorIds.Length; i++)
        {
            _detectors[i] = trackDesigned.DetectorIds[i];
            _detectorPositions[trackDesigned.DetectorIds[i]] = i;
        }
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
        List<object> newEvents = [];
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
    public void SetMinimumDetectionDelay(long minimumMilliseconds)
    {
        Raise(new TrackRedetectionDelayConfigured(Id, minimumMilliseconds));
    }
    public void ResetMaximumLapTime()
    {
        Raise(new TrackMaximumLapTimeReset(Id));
    }
    public void ResetMinimumDetectionDelay()
    {
        Raise(new TrackRedetectionDelayReset(Id));
    }
}

