using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.SQLite.Context;
using NaeTime.Persistence.SQLite.Models;

namespace NaeTime.Persistence.SQLite;
public class SQLiteTrackRepository : ITrackRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLiteTrackRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task AddOrUpdateTrack(Guid id, string name, long minimumLapMilliseconds, long maximumLapMilliseconds, IEnumerable<Guid> timers)
    {
        var existing = await _dbContext.Tracks.FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null)
        {
            existing = new Track
            {
                Id = id,
                Name = name,
                MinimumLapMilliseconds = minimumLapMilliseconds,
                MaximumLapMilliseconds = maximumLapMilliseconds,
                Timers = timers.Select(x => new TrackTimer { TrackId = id, TimerId = x }).ToList()
            };

            _dbContext.Tracks.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.MinimumLapMilliseconds = minimumLapMilliseconds;
            existing.MaximumLapMilliseconds = maximumLapMilliseconds;
            existing.Timers = timers.Select(x => new TrackTimer { TrackId = id, TimerId = x }).ToList();
        }

        await _dbContext.SaveChangesAsync();
    }
}
