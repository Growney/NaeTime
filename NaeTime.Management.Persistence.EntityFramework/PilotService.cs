namespace NaeTime.Management.Persistence.EntityFramework;
internal class PilotService
{
    private readonly NaeTimeDbContext _dbContext;

    public PilotService(NaeTimeDbContext dbContext)
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
        Pilot? existing = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == pilot.PilotId).ConfigureAwait(false);
        if (existing == null)
        {
            return;
        }

        existing.FirstName = pilot.FirstName;
        existing.LastName = pilot.LastName;
        existing.CallSign = pilot.CallSign;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
