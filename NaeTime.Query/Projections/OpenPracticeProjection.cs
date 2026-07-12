using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class OpenPracticeProjection : IOpenPracticeProjection
{
    private class LaneSnapshot
    {
        public byte Lane { get; set; }
        public Guid? PilotId { get; set; }
        public bool IsEnabled { get; set; }
        public byte? BandId { get; set; }
        public int FrequencyInMHz { get; set; }
    }

    private class SessionSnapshot
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TrackId { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<Guid, bool> AttendingPilots { get; set; } = new();
        public List<LaneSnapshot> Lanes { get; set; } = [];
        public TimeSpan? MinimumLapTime { get; set; }
        public TimeSpan? MaximumLapTime { get; set; }
    }

    private class ProjectionSnapshot
    {
        public Dictionary<Guid, SessionSnapshot> Sessions { get; set; } = new();
        // sessionId -> pilotId -> minimum lap time
        public Dictionary<Guid, Dictionary<Guid, TimeSpan>> SessionPilotMinimumLapTimes { get; set; } = new();
        // sessionId -> pilotId -> maximum lap time
        public Dictionary<Guid, Dictionary<Guid, TimeSpan>> SessionPilotMaximumLapTimes { get; set; } = new();
    }

    private class ListOpenPracticeSessionLane
    {
        public byte Lane { get; set; }
        public Guid? PilotId { get; set; }
        public bool IsEnabled { get; set; }
        public byte? BandId { get; set; }
        public int FrequencyInMHz { get; set; }

        public override string ToString() => $"Lane {Lane}, PilotId: {PilotId}, IsEnabled: {IsEnabled}, BandId: {BandId}, FrequencyInMHz: {FrequencyInMHz}";
    }
    private class ListOpenPracticeSession
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TrackId { get; set; }
        public bool IsActive { get; set; }
        public ConcurrentDictionary<Guid, bool> AttendingPilots { get; set; } = new();
        public ConcurrentBag<ListOpenPracticeSessionLane> Lanes { get; set; } = [];
        public TimeSpan? MinimumLapTime { get; set; }
        public TimeSpan? MaximumLapTime { get; set; }

        public override string ToString() => $"Session {Id}, Name: {Name}, TrackId: {TrackId}";

    }
    private readonly ConcurrentDictionary<Guid, ListOpenPracticeSession> _sessions = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, TimeSpan>> _sessionPilotMinimumLapTimes = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, TimeSpan>> _sessionPilotMaximumLapTimes = new();

    public OpenPracticeSession? GetSession(Guid sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return new OpenPracticeSession(session.Id, session.Name, session.TrackId, session.IsActive, session.MinimumLapTime, session.MaximumLapTime, session.AttendingPilots.Keys, [.. session.Lanes.OrderBy(x => x.Lane).Select(l => new OpenPracticeLane(l.Lane, l.PilotId, l.IsEnabled, l.BandId, l.FrequencyInMHz))]);
        }
        return null;
    }
    private void When(OpenPracticeSessionActivated activated)
    {
        if (_sessions.TryGetValue(activated.SessionId, out var session))
        {
            session.IsActive = true;
        }
    }
    private void When(OpenPracticeSessionDeactivated deactivated)
    {
        if (_sessions.TryGetValue(deactivated.SessionId, out var session))
        {
            session.IsActive = false;
        }
    }
    private void When(OpenPracticeSessionScheduled scheduled)
    {
        _sessions.GetOrAdd(scheduled.SessionId, id => new ListOpenPracticeSession
        {
            Id = scheduled.SessionId,
            Name = scheduled.Name,
            TrackId = scheduled.TrackId,
            MinimumLapTime = scheduled.MinimumLapTime,
            MaximumLapTime = scheduled.MaximumLapTime
        });
    }
    private void When(OpenPracticeSessionRenamed renamed)
    {
        if (_sessions.TryGetValue(renamed.Sessionid, out var session))
        {
            session.Name = renamed.Name;
        }
    }
    private void When(OpenPracticeSessionLanePilotSet lanePilotSet)
    {
        if (_sessions.TryGetValue(lanePilotSet.SessionId, out var session))
        {
            session.AttendingPilots.TryAdd(lanePilotSet.PilotId, true);

            var lane = session.Lanes.FirstOrDefault(l => l.Lane == lanePilotSet.Lane);
            if (lane == null)
            {
                lane = new ListOpenPracticeSessionLane
                {
                    Lane = lanePilotSet.Lane,
                    IsEnabled = true
                };
                session.Lanes.Add(lane);
            }
            lane.PilotId = lanePilotSet.PilotId;
        }
    }
    private void When(OpenPracticeSessionLanePilotReset lanePilotReset)
    {
        if (_sessions.TryGetValue(lanePilotReset.SessionId, out var session))
        {
            var lane = session.Lanes.FirstOrDefault(l => l.Lane == lanePilotReset.Lane);
            if (lane != null)
            {
                lane.PilotId = null;
            }
        }
    }
    private void When(OpenPracticeSessionLaneVideoFrequencyTuned laneVideoFrequencyTuned)
    {
        if (_sessions.TryGetValue(laneVideoFrequencyTuned.SessionId, out var session))
        {
            var lane = session.Lanes.FirstOrDefault(l => l.Lane == laneVideoFrequencyTuned.Lane);
            if (lane == null)
            {
                lane = new ListOpenPracticeSessionLane
                {
                    Lane = laneVideoFrequencyTuned.Lane,
                    IsEnabled = true
                };
                session.Lanes.Add(lane);
            }
            lane.BandId = laneVideoFrequencyTuned.BandId;
            lane.FrequencyInMHz = laneVideoFrequencyTuned.FrequencyInMHz;
        }
    }

    private void When(OpenPracticeSessionLaneStatusSet statusSet)
    {
        if (_sessions.TryGetValue(statusSet.SessionId, out var session))
        {
            var lane = session.Lanes.FirstOrDefault(l => l.Lane == statusSet.Lane);
            if (lane == null)
            {
                lane = new ListOpenPracticeSessionLane
                {
                    Lane = statusSet.Lane,
                };
                session.Lanes.Add(lane);
            }
            lane.IsEnabled = statusSet.IsEnabled;
        }
    }
    private void When(OpenPracticeSessionPilotMinimumLapTimeSet set)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotTimes = _sessionPilotMinimumLapTimes.GetOrAdd(set.SessionId, _ => new());
        pilotTimes[set.PilotId] = set.MinimumLapTime;
    }
    private void When(OpenPracticeSessionPilotMinimumLapTimeReset reset)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotTimes = _sessionPilotMinimumLapTimes.GetOrAdd(reset.SessionId, _ => new());
        pilotTimes.TryRemove(reset.PilotId, out _);
    }
    private void When(OpenPracticeSessionPilotMaximumLapTimeSet set)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotTimes = _sessionPilotMaximumLapTimes.GetOrAdd(set.SessionId, _ => new());
        pilotTimes[set.PilotId] = set.MaximumLapTime;
    }
    private void When(OpenPracticeSessionPilotMaximumLapTimeReset reset)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotTimes = _sessionPilotMaximumLapTimes.GetOrAdd(reset.SessionId, _ => new());
        pilotTimes.TryRemove(reset.PilotId, out _);
    }
    public (TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime) GetPilotLapTimeOverrides(Guid sessionId, Guid pilotId)
    {
        TimeSpan? min = null;
        TimeSpan? max = null;
        if (_sessionPilotMinimumLapTimes.TryGetValue(sessionId, out var minTimes) && minTimes.TryGetValue(pilotId, out var minTime))
            min = minTime;
        if (_sessionPilotMaximumLapTimes.TryGetValue(sessionId, out var maxTimes) && maxTimes.TryGetValue(pilotId, out var maxTime))
            max = maxTime;
        return (min, max);
    }

    private ProjectionSnapshot Snapshot()
    {
        var snapshot = new ProjectionSnapshot();

        foreach (var (sessionId, session) in _sessions)
        {
            snapshot.Sessions[sessionId] = new SessionSnapshot
            {
                Id = session.Id,
                Name = session.Name,
                TrackId = session.TrackId,
                IsActive = session.IsActive,
                AttendingPilots = new Dictionary<Guid, bool>(session.AttendingPilots),
                Lanes = [.. session.Lanes.Select(l => new LaneSnapshot
                {
                    Lane = l.Lane,
                    PilotId = l.PilotId,
                    IsEnabled = l.IsEnabled,
                    BandId = l.BandId,
                    FrequencyInMHz = l.FrequencyInMHz,
                })],
                MinimumLapTime = session.MinimumLapTime,
                MaximumLapTime = session.MaximumLapTime,
            };
        }

        foreach (var (sessionId, pilotMap) in _sessionPilotMinimumLapTimes)
        {
            snapshot.SessionPilotMinimumLapTimes[sessionId] = new Dictionary<Guid, TimeSpan>(pilotMap);
        }

        foreach (var (sessionId, pilotMap) in _sessionPilotMaximumLapTimes)
        {
            snapshot.SessionPilotMaximumLapTimes[sessionId] = new Dictionary<Guid, TimeSpan>(pilotMap);
        }

        return snapshot;
    }

    private void Restore(ProjectionSnapshot snapshot)
    {
        _sessions.Clear();
        _sessionPilotMinimumLapTimes.Clear();
        _sessionPilotMaximumLapTimes.Clear();

        foreach (var (sessionId, sessionSnapshot) in snapshot.Sessions)
        {
            var session = new ListOpenPracticeSession
            {
                Id = sessionSnapshot.Id,
                Name = sessionSnapshot.Name,
                TrackId = sessionSnapshot.TrackId,
                IsActive = sessionSnapshot.IsActive,
                AttendingPilots = new ConcurrentDictionary<Guid, bool>(sessionSnapshot.AttendingPilots),
                Lanes = [],
                MinimumLapTime = sessionSnapshot.MinimumLapTime,
                MaximumLapTime = sessionSnapshot.MaximumLapTime,
            };
            foreach (var laneSnapshot in sessionSnapshot.Lanes)
            {
                session.Lanes.Add(new ListOpenPracticeSessionLane
                {
                    Lane = laneSnapshot.Lane,
                    PilotId = laneSnapshot.PilotId,
                    IsEnabled = laneSnapshot.IsEnabled,
                    BandId = laneSnapshot.BandId,
                    FrequencyInMHz = laneSnapshot.FrequencyInMHz,
                });
            }
            _sessions[sessionId] = session;
        }

        foreach (var (sessionId, pilotMap) in snapshot.SessionPilotMinimumLapTimes)
        {
            var concurrentMap = _sessionPilotMinimumLapTimes.GetOrAdd(sessionId, _ => new());
            foreach (var (pilotId, lapTime) in pilotMap)
            {
                concurrentMap[pilotId] = lapTime;
            }
        }

        foreach (var (sessionId, pilotMap) in snapshot.SessionPilotMaximumLapTimes)
        {
            var concurrentMap = _sessionPilotMaximumLapTimes.GetOrAdd(sessionId, _ => new());
            foreach (var (pilotId, lapTime) in pilotMap)
            {
                concurrentMap[pilotId] = lapTime;
            }
        }
    }
}
