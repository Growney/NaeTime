using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class SQLiteSessionRepository : ISessionRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLiteSessionRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task<IEnumerable<SessionDetails>> GetDetails()
    {
        var details = new List<SessionDetails>();

        var openPracticeSessions = await _dbContext.OpenPracticeSessions.Select(x => new SessionDetails(x.Id, x.Name, SessionType.OpenPractice, x.TrackId, x.MinimumLapMilliseconds, x.MaximumLapMilliseconds)).ToListAsync().ConfigureAwait(false);

        details.AddRange(openPracticeSessions);

        return details;
    }
    public async Task<SessionDetails?> GetDetails(Guid sessionId)
    {
        var openPracticeSession = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        if (openPracticeSession == null)
        {
            return null;
        }

        return new SessionDetails(openPracticeSession.Id, openPracticeSession.Name, SessionType.OpenPractice, openPracticeSession.TrackId, openPracticeSession.MinimumLapMilliseconds, openPracticeSession.MaximumLapMilliseconds);
    }
}
