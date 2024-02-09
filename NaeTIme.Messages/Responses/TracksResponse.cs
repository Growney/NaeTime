namespace NaeTime.Messages.Responses;
public record TracksResponse(IEnumerable<TracksResponse.Track> Tracks)
{
    public record Track(Guid Id, string Name, long MinimumLapTimeMilliseconds, long? MaximumLapTimeMilliseconds, IEnumerable<Guid> Timers, byte AllowedLanes);
}
