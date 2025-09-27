using EventDbLite.Projections;
using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;

public class DetectorList : Projection
{
    private readonly ConcurrentDictionary<Guid, Detector> _detectors = new();

    public void When(NaeTimeNodeSerialEsp32NodeConfigured e)
    {
        _detectors[e.TimerId] = new Detector(e.TimerId, e.Name, DetectorType.NaeTimeSerial, e.Lanes);
    }

    public void When(NaeTimeNodeTimerConnected e)
    {
        if (_detectors.TryGetValue(e.TimerId, out var detector))
            _detectors[e.TimerId] = detector with { };
    }

    public void When(NaeTimeNodeTimerDisconnected e)
    {
        // Optionally handle disconnection if needed
    }

    public void When(ImmersionRCLapRFNetworkDeviceRegistered e)
    {
        _detectors[e.TimerId] = new Detector(e.TimerId, null, DetectorType.EthernetLapRF8Channel, e.Lanes);
    }

    public void When(ImmersionRCLapRFRenamed e)
    {
        if (_detectors.TryGetValue(e.TimerId, out var detector))
            _detectors[e.TimerId] = detector with { Name = e.Name };
    }

    public IEnumerable<Detector> GetDetectors() => _detectors.Values;
    public Detector? GetDetector(Guid id) => _detectors.TryGetValue(id, out var detector) ? detector : null;
    public IEnumerable<Detector> GetDetectors(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            if (_detectors.TryGetValue(id, out var detector))
            {
                yield return detector;
            }
        }
    }
}