using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Query;
public class SessionQueryHandler : ISessionQueryHandler
{
    private readonly ISessionProjection _sessionProjection;

    public SessionQueryHandler(ISessionProjection sessionProjection)
    {
        _sessionProjection = sessionProjection;
    }

    public Task<IEnumerable<Session>> GetAllSessions() => Task.FromResult(_sessionProjection.GetAllSessions());

    public Task<Session?> GetSession(Guid id) => Task.FromResult(_sessionProjection.GetSessionById(id));

    public Task<Session?> GetActiveSession() => Task.FromResult(_sessionProjection.GetActiveSession());
}
