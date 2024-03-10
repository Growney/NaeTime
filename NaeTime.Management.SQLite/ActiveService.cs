
using NaeTime.Management.Messages.Messages;
using NaeTime.Management.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Persistence;
internal class ActiveService : ISubscriber
{
    private readonly ManagementDbContext _dbContext;

    public ActiveService(ManagementDbContext dbContext, IDispatcher dispatcher)
    {
        _dbContext = dbContext;
    }
    public async Task When(SessionActivated activated)
    {
        var active = await _dbContext.ActiveSession.FirstOrDefaultAsync();

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

    public async Task<ActiveSessionResponse?> On(ActiveSessionRequest _)
    {
        var active = await _dbContext.ActiveSession.FirstOrDefaultAsync();
        if (active == null)
        {
            return null;
        }

        return new ActiveSessionResponse(active.SessionId, active.SessionType switch
        {
            SessionType.OpenPractice => ActiveSessionResponse.SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        });
    }
}
