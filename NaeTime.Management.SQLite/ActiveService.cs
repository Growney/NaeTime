
using NaeTime.Management.Messages.Messages;
using NaeTime.Management.Messages.Responses;

namespace NaeTime.Persistence;
internal class ActiveService : ISubscriber
{
    private readonly ManagementDbContext _dbContext;

    public ActiveService(ManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task When(SessionActivated activated)
    {
        var active = await _dbContext.ActiveSession.FirstOrDefaultAsync();

        active ??= new ActiveSession
        {
            Id = Guid.NewGuid(),
        };

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

    public async Task<ActiveSessionResponse?> On(ActiveSessionRequest _)
    {
        var active = await _dbContext.ActiveSession.FirstOrDefaultAsync();

        return active == null
            ? null
            : new ActiveSessionResponse(active.SessionId, active.SessionType switch
            {
                SessionType.OpenPractice => ActiveSessionResponse.SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            });
    }
}
