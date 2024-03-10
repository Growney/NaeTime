using NaeTime.Management.Messages.Messages;
using NaeTime.Management.Messages.Responses;

namespace NaeTime.Management.SQLite;
internal class PilotService : ISubscriber
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

    public async Task<PilotsResponse> On(PilotsRequest request)
    {
        var pilots = await _dbContext.Pilots.Select(x => new PilotsResponse.Pilot(x.Id, x.FirstName, x.LastName, x.CallSign))
            .ToListAsync().ConfigureAwait(false);

        return new PilotsResponse(pilots);
    }

    public async Task<PilotResponse?> On(PilotRequest request)
    {
        var pilot = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == request.PilotId).ConfigureAwait(false);

        return pilot == null ? null : new PilotResponse(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign);
    }
}
