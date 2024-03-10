namespace NaeTime.OpenPractice.SQLite;
internal class SessionService : ISubscriber
{
    private readonly OpenPracticeDbContext _dbContext;

    public SessionService(OpenPracticeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<OpenPracticeSessionResponse?> On(OpenPracticeSessionRequest request)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == request.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }

        var laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == request.SessionId).ToListAsync().ConfigureAwait(false);


        return session == null ?
            null : new OpenPracticeSessionResponse(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds,
            laps.Select(x => new OpenPracticeSessionResponse.Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc,
            x.Status switch
            {
                OpenPracticeLapStatus.Invalid => OpenPracticeSessionResponse.LapStatus.Invalid,
                OpenPracticeLapStatus.Completed => OpenPracticeSessionResponse.LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, x.TotalMilliseconds)),
            session.ActiveLanes.Select(x => new OpenPracticeSessionResponse.PilotLane(x.PilotId, x.Lane)),
            session.TrackedConsecutiveLaps.Select(x => x.LapCap));
    }
}
