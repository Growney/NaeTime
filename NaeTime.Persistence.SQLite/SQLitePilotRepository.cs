using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class PilotRepository : IPilotRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public PilotRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task AddOrUpdatePilot(Guid id, string firstName, string lastName, string callSign)
    {
        var existing = await _dbContext.Pilots.FirstOrDefaultAsync();

        if (existing == null)
        {
            existing = new Models.Pilot
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                CallSign = callSign
            };

            _dbContext.Pilots.Add(existing);
        }
        else
        {
            existing.FirstName = firstName;
            existing.LastName = lastName;
            existing.CallSign = callSign;
        }

        await _dbContext.SaveChangesAsync();
    }
}
