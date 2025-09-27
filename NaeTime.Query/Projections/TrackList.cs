using EventDbLite.Projections;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;

public class TrackList : Projection
{
    private class ListTrack
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid[] DetectorIds { get; set; } = Array.Empty<Guid>();
        public long? MinimumLapMilliseconds { get; set; }
        public long? MaximumLapMilliseconds { get; set; }
    }

    private readonly ConcurrentDictionary<Guid, ListTrack> _tracks = new();

    private readonly IHardwareQueryHandler _hardwareQueryHandler;

    public TrackList(IHardwareQueryHandler hardwareQueryHandler)
    {
        _hardwareQueryHandler = hardwareQueryHandler;
    }

    public async Task<Track?> GetTrack(Guid id)
    {
        _tracks.TryGetValue(id, out var listTrack);

        if (listTrack == null)
        {
            return null;
        }

        IEnumerable<Detector> detectors = await _hardwareQueryHandler.GetDetectors(listTrack.DetectorIds);

        byte maxLanes = detectors.Any() ? detectors.Max(d => d.SupportedLanes) : (byte)0;

        return new Track(id, listTrack.Name, detectors.ToArray(), maxLanes, listTrack.MinimumLapMilliseconds, listTrack.MaximumLapMilliseconds);

    }
    public async Task<IEnumerable<Track>> GetTracks()
    {
        var tasks = _tracks.Values.Select(async listTrack =>
        {
            IEnumerable<Detector> detectors = await _hardwareQueryHandler.GetDetectors(listTrack.DetectorIds);
            byte maxLanes = detectors.Any() ? detectors.Max(d => d.SupportedLanes) : (byte)0;
            return new Track(listTrack.Id, listTrack.Name, detectors.ToArray(), maxLanes, (int)listTrack.MinimumLapMilliseconds, (int)(listTrack.MaximumLapMilliseconds ?? int.MaxValue));
        });
        return await Task.WhenAll(tasks);
    }
}