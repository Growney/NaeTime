using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query;
public class SessionQueryHandler(IProjectionProvider projectionProvider) : ISessionQueryHandler
{
    private readonly IProjectionProvider _projectionProvider = projectionProvider;

    public async Task<IEnumerable<Session>> GetAllSessions()
    {
        SessionList sessionList = await _projectionProvider.Load<SessionList>();
        return sessionList.GetAllSessions();
    }

    public async Task<Session?> GetSession(Guid id)
    {
        SessionList sessionList = await _projectionProvider.Load<SessionList>();
        return sessionList.GetSessionById(id);
    }

    public async Task<Session?> GetActiveSession()
    {
        SessionList sessionList = await _projectionProvider.Load<SessionList>();
        return sessionList.GetActiveSession();
    }
}
