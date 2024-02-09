using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;
using System.Data;

namespace NaeTime.Persistence.SQLite;
public class SQLiteTrackRepository : ITrackRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLiteTrackRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task AddOrUpdateTrack(Guid id, string name, long minimumLapMilliseconds, long? maximumLapMilliseconds, IEnumerable<Guid> timers, byte allowedLanes)
    {
        var existing = await _dbContext.Tracks.FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null)
        {
            existing = new Models.Track
            {
                Id = id,
                Name = name,
                MinimumLapMilliseconds = minimumLapMilliseconds,
                MaximumLapMilliseconds = maximumLapMilliseconds,
                Timers = timers.Select(x => new Models.TrackTimer { Id = Guid.NewGuid(), TrackId = id, TimerId = x }).ToList(),
                AllowedLanes = allowedLanes
            };

            _dbContext.Tracks.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.MinimumLapMilliseconds = minimumLapMilliseconds;
            existing.MaximumLapMilliseconds = maximumLapMilliseconds;
            existing.Timers = timers.Select(x => new Models.TrackTimer { Id = Guid.NewGuid(), TrackId = id, TimerId = x }).ToList();
            existing.AllowedLanes = allowedLanes;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Track?> Get(Guid id)
    {
        var existing = await _dbContext.Tracks.FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null)
        {
            return null;
        }

        return new Track(existing.Id, existing.Name, existing.MinimumLapMilliseconds, existing.MaximumLapMilliseconds, existing.Timers.Select(x => x.TimerId), existing.AllowedLanes);
    }

    public async Task<IEnumerable<Track>> GetAll()
    {
        return await _dbContext.Tracks.Select(x => new Track(x.Id, x.Name, x.MinimumLapMilliseconds, x.MaximumLapMilliseconds, x.Timers.Select(y => y.TimerId), x.AllowedLanes)).ToListAsync();
    }

}
