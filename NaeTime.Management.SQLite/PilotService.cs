using NaeTime.Management.Messages.Messages;

namespace NaeTime.Management.SQLite;
internal class PilotService
{
    private readonly ManagementDbContext _dbContext;

    public PilotService(ManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task When(PilotCreated pilot)
    {
        _dbContext.Pilots.Add(new Pilot()
        {
            Id = pilot.PilotId,
            FirstName = pilot.FirstName,
            LastName = pilot.LastName,
            CallSign = pilot.CallSign
        });
        await _dbContext.SaveChangesAsync();
    }
    public async Task When(PilotDetailsChanged pilot)
    {
        var existing = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == pilot.PilotId).ConfigureAwait(false);
        if (existing == null)
        {
            return;
        }

        existing.FirstName = pilot.FirstName;
        existing.LastName = pilot.LastName;
        existing.CallSign = pilot.CallSign;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Messages.Models.Pilot>> GetPilots()
    {
        var pilots = await _dbContext.Pilots.Select(x => new Messages.Models.Pilot(x.Id, x.FirstName, x.LastName, x.CallSign))
            .ToListAsync().ConfigureAwait(false);

        return pilots;
    }

    public async Task<Messages.Models.Pilot?> GetPilot(Guid pilotId)
    {
        var pilot = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == pilotId).ConfigureAwait(false);

        return pilot == null ? null : new Messages.Models.Pilot(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign);
    }
}
