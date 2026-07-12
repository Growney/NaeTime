using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;

public class TrackProjection : ITrackProjection
{
    private class ListTrack
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid[] DetectorIds { get; set; } = Array.Empty<Guid>();
    }

    private readonly ConcurrentDictionary<Guid, ListTrack> _tracks = new();

    private readonly IHardwareQueryHandler _hardwareQueryHandler;

    public TrackProjection(IHardwareQueryHandler hardwareQueryHandler)
    {
        _hardwareQueryHandler = hardwareQueryHandler;
    }

    public async Task<Track?> GetTrack(Guid id)
    {
        _tracks.TryGetValue(id, out ListTrack? listTrack);

        if (listTrack == null)
        {
            return null;
        }

        IEnumerable<Detector> detectors = await _hardwareQueryHandler.GetDetectors(listTrack.DetectorIds);

        byte maxLanes = detectors.Any() ? detectors.Max(d => d.SupportedLanes) : (byte)0;

        return new Track(id, listTrack.Name, detectors.ToArray(), maxLanes);

    }
    public async Task<IEnumerable<Track>> GetTracks()
    {
        IEnumerable<Task<Track>> tasks = _tracks.Values.Select(async listTrack =>
        {
            IEnumerable<Detector> detectors = await _hardwareQueryHandler.GetDetectors(listTrack.DetectorIds);
            byte maxLanes = detectors.Any() ? detectors.Max(d => d.SupportedLanes) : (byte)0;
            return new Track(listTrack.Id, listTrack.Name, detectors.ToArray(), maxLanes);
        });
        return await Task.WhenAll(tasks);
    }
    private void When(TrackDesigned designed)
    {
        ListTrack track = _tracks.GetOrAdd(designed.TrackId, id => new ListTrack { Id = id });
        track.Name = designed.Name;
        track.DetectorIds = designed.DetectorIds;
    }
    private void When(TrackRenamed renamed)
    {
        ListTrack track = _tracks.GetOrAdd(renamed.TrackId, id => new ListTrack { Id = id });
        track.Name = renamed.Name;
    }
    private void When(TrackDetectorAdded detectorAdded)
    {
        ListTrack track = _tracks.GetOrAdd(detectorAdded.TrackId, id => new ListTrack { Id = id });
        List<Guid> detectorIds = track.DetectorIds.ToList();
        if (detectorAdded.OrdinalPosition <= detectorIds.Count)
        {
            detectorIds.Insert(detectorAdded.OrdinalPosition, detectorAdded.DetectorId);
        }
        else
        {
            detectorIds.Add(detectorAdded.DetectorId);
        }
        track.DetectorIds = detectorIds.ToArray();
    }
    private void When(TrackDetectorRemoved detectorRemoved)
    {
        ListTrack track = _tracks.GetOrAdd(detectorRemoved.TrackId, id => new ListTrack { Id = id });
        List<Guid> detectorIds = track.DetectorIds.ToList();
        detectorIds.Remove(detectorRemoved.DetectorId);
        track.DetectorIds = detectorIds.ToArray();
    }
    private void When(TrackDetectorMoved detectorMoved)
    {
        ListTrack track = _tracks.GetOrAdd(detectorMoved.TrackId, id => new ListTrack { Id = id });
        List<Guid> detectorIds = track.DetectorIds.ToList();
        detectorIds.Remove(detectorMoved.DetectorId);
        if (detectorMoved.OrdinalPosition <= detectorIds.Count)
        {
            detectorIds.Insert(detectorMoved.OrdinalPosition, detectorMoved.DetectorId);
        }
        else
        {
            detectorIds.Add(detectorMoved.DetectorId);
        }
        track.DetectorIds = detectorIds.ToArray();
    }
}