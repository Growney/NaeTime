using NaeTime.Management.Messages.Messages;

namespace NaeTime.Management.SQLite;
internal class TrackService
{
    private readonly ManagementDbContext _dbContext;

    public TrackService(ManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task When(TrackCreated trackCreated)
    {
        _dbContext.Tracks.Add(new Track()
        {
            Id = trackCreated.Id,
            Name = trackCreated.Name,
            MinimumLapMilliseconds = trackCreated.MinimumLapMilliseconds,
            MaximumLapMilliseconds = trackCreated.MaximumLapMilliseconds,
            Timers = trackCreated.Timers.Select(x => new TrackTimer
            {
                Id = Guid.NewGuid(),
                TimerId = x,
                TrackId = trackCreated.Id
            }).ToList(),
            AllowedLanes = trackCreated.MaxLanes
        });

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Messages.Models.Track>> GetTracks()
    {
        var tracks = await _dbContext.Tracks.Select(x => new Messages.Models.Track(x.Id, x.Name, x.MinimumLapMilliseconds, x.MaximumLapMilliseconds, x.Timers.Select(y => y.TimerId).ToList(), x.AllowedLanes))
            .ToListAsync().ConfigureAwait(false);

        return tracks;
    }

    public async Task<Messages.Models.Track?> GetTrack(Guid trackId)
    {
        var track = await _dbContext.Tracks.FirstOrDefaultAsync(x => x.Id == trackId).ConfigureAwait(false);

        return track == null
            ? null
            : new Messages.Models.Track(track.Id, track.Name, track.MinimumLapMilliseconds, track.MaximumLapMilliseconds, track.Timers.Select(x => x.TimerId).ToList(), track.AllowedLanes);
    }
}
