using EventDbLite.Projections;
using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Projections;
public class SessionList : Projection
{
    private class ListSession
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Abstractions.Models.SessionType Type { get; set; }
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

    private void When(SessionActivated e)
    {
        _activeSessionId = e.SessionId;
    }

    private void When(SessionDeactivated e)
    {
        if (_activeSessionId == e.SessionId)
        {
            _activeSessionId = null;
        }
    }

    private void When(SessionScheduled scheduled)
    {
        _sessions.GetOrAdd(scheduled.SessionId, id => new ListSession()
        {
            Id = scheduled.SessionId,
            Name = scheduled.Name,
            Type = scheduled.SessionType switch
            {
                Events.SessionType.OpenPractice => Abstractions.Models.SessionType.OpenPractice,
                _ => throw new NotImplementedException($"Unsupported session type: {scheduled.SessionType}")
            }
        });
    }
    private void When(SessionRenamed renamed)
    {
        if (_sessions.TryGetValue(renamed.Sessionid, out var session))
        {
            session.Name = renamed.Name;
        }
    }
}
