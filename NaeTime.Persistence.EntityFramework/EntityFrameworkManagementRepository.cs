using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions.Management;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkManagementRepository : IManagementRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public EntityFrameworkManagementRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<ActiveSession?> GetActiveSession()
    {
        var active = await _dbContext.ActiveSession.FirstOrDefaultAsync();
        if (active == null)
        {
            return null;
        }

        return new ActiveSession(active.SessionId, active.SessionType switch
        {
            Models.SessionType.OpenPractice => ActiveSession.SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        });
    }

    public async Task<Pilot?> GetPilot(Guid pilotId)
    {
        var pilot = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == pilotId).ConfigureAwait(false);

        return pilot == null ? null : new Pilot(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign);
    }
    public async Task<IEnumerable<Pilot>> GetPilots() => await _dbContext.Pilots.Select(x => new Pilot(x.Id, x.FirstName, x.LastName, x.CallSign)).ToListAsync();
    public async Task<Track?> GetTrack(Guid trackId)
    {
        var track = await _dbContext.Tracks.FirstOrDefaultAsync(x => x.Id == trackId).ConfigureAwait(false);

        return track == null
            ? null
            : new Track(track.Id, track.Name, track.MinimumLapMilliseconds, track.MaximumLapMilliseconds, track.Timers.Select(x => x.TimerId).ToList(), track.AllowedLanes);
    }
    public async Task<IEnumerable<Track>> GetTracks() => await _dbContext.Tracks.Select(x => new Track(x.Id, x.Name, x.MinimumLapMilliseconds, x.MaximumLapMilliseconds, x.Timers.Select(y => y.TimerId).ToList(), x.AllowedLanes)).ToListAsync();

}
