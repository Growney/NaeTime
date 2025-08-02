using NaeTime.Query.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;

public class TrackList
{
    private class ListTrack
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid[] DetectorIds { get; set; } = Array.Empty<Guid>();
        public long MinimumLapMilliseconds { get; set; }
        public long? MaximumLapMilliseconds { get; set; }
    }

    private readonly ConcurrentDictionary<Guid, ListTrack> _tracks = new();

    private readonly IHardwareQueryHandler _hardwareQueryHandler;

    public TrackList(IHardwareQueryHandler hardwareQueryHandler)
    {
        _hardwareQueryHandler = hardwareQueryHandler;
    }

}