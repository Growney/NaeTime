using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class SessionProjection : ISessionProjection
{
    private class ListSession
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SessionType Type { get; set; }
    }

    private Guid? _activeSessionId = null;

    private ConcurrentDictionary<Guid, ListSession> _sessions = new();

    public IEnumerable<Session> GetAllSessions()
    {
        return _sessions.Values
            .Select(s => new Session(s.Id, s.Name, s.Type, s.Id == _activeSessionId ? true : false));
    }
    public Session? GetSessionById(Guid sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return new Session(session.Id, session.Name, session.Type, session.Id == _activeSessionId ? true : false);
        }
        return null;
    }
    public Session? GetActiveSession()
    {
        if (_activeSessionId.HasValue && _sessions.TryGetValue(_activeSessionId.Value, out var session))
        {
            return new Session(session.Id, session.Name, session.Type, true);
        }
        return null;
    }

    private void When(OpenPracticeSessionActivated e)
    {
        _activeSessionId = e.SessionId;
    }

    private void When(OpenPracticeSessionDeactivated e)
    {
        if (_activeSessionId == e.SessionId)
        {
            _activeSessionId = null;
        }
    }

    private void When(OpenPracticeSessionScheduled scheduled)
    {
        _sessions.GetOrAdd(scheduled.SessionId, id => new ListSession()
        {
            Id = scheduled.SessionId,
            Name = scheduled.Name,
            Type = SessionType.OpenPractice
        });
    }
    private void When(OpenPracticeSessionRenamed renamed)
    {
        if (_sessions.TryGetValue(renamed.Sessionid, out var session))
        {
            session.Name = renamed.Name;
        }
    }
}
