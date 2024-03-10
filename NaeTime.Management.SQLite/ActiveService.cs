
using NaeTime.Management.Messages.Messages;
using NaeTime.Management.Messages.Responses;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Persistence;
internal class ActiveService : ISubscriber
{
    private readonly ManagementDbContext _dbContext;
    private readonly IDispatcher _dispatcher;

    public ActiveService(ManagementDbContext dbContext, IDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
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
        if (active == null)
        {
            return null;
        }

        if (active.SessionType == SessionType.OpenPractice)
        {
            var session = await _dispatcher.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(active.SessionId));

            if (session == null)
            {
                return null;
            }

            var sessionTrack = await _dbContext.Tracks.FirstOrDefaultAsync(x => x.Id == session.TrackId);

            if (sessionTrack == null)
            {
                return null;
            }

            return new ActiveSessionResponse(active.SessionId, active.SessionType switch
            {
                SessionType.OpenPractice => ActiveSessionResponse.SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            }, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, new ActiveSessionResponse.Track(sessionTrack.Id, sessionTrack.AllowedLanes, sessionTrack.Timers.Select(x => x.TimerId)));
        }

        return null;
    }
}
