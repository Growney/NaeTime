namespace NaeTime.Management.Persistence.EntityFramework;
internal class TrackService
{
    private readonly NaeTimeDbContext _dbContext;

    public TrackService(NaeTimeDbContext dbContext)
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
}
