namespace NaeTime.Management.Persistence.EntityFramework;
internal class ActiveService
{
    private readonly NaeTimeDbContext _dbContext;

    public ActiveService(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task When(SessionActivated activated)
    {
        ActiveSession? active = await _dbContext.ActiveSession.FirstOrDefaultAsync();

        if (active == null)
        {
            active = new ActiveSession
            {
                Id = Guid.NewGuid(),
            };
            _dbContext.ActiveSession.Add(active);
        }

        active.SessionId = activated.SessionId;
        active.SessionType = activated.Type switch
        {
            SessionActivated.SessionType.OpenPractice => SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        };


        await _dbContext.SaveChangesAsync();
    }
    public async Task When(SessionDeactivated _)
    {
        _dbContext.ActiveSession.RemoveRange(_dbContext.ActiveSession);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Messages.Models.ActiveSession?> GetActiveSession()
    {
        ActiveSession? active = await _dbContext.ActiveSession.FirstOrDefaultAsync();
        if (active == null)
        {
            return null;
        }

        return new Messages.Models.ActiveSession(active.SessionId, active.SessionType switch
        {
            SessionType.OpenPractice => Messages.Models.ActiveSession.SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        });
    }
}
