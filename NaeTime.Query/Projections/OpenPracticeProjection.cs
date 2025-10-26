using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class OpenPracticeProjection : IOpenPracticeProjection
{
    private class ListOpenPracticeSessionLane
    {
        public byte Lane { get; set; }
        public Guid? PilotId { get; set; }
        public bool IsEnabled { get; set; }
        public byte? BandId { get; set; }
        public int FrequencyInMHz { get; set; }
    }
    private class ListOpenPracticeSession
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TrackId { get; set; }
        public IEnumerable<Guid> TrackDetectorIds { get; set; } = Enumerable.Empty<Guid>();
        public List<ListOpenPracticeSessionLane> Lanes { get; set; } = [];

    }
    private readonly ConcurrentDictionary<Guid, ListOpenPracticeSession> _sessions = new();

    public OpenPracticeSession? GetSession(Guid sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return new OpenPracticeSession(session.Id, session.Name, session.TrackId, session.TrackDetectorIds, [.. session.Lanes.Select(l => new OpenPracticeLane(l.Lane, l.PilotId, l.IsEnabled, l.BandId, l.FrequencyInMHz))]);
        }
        return null;
    }

    private void When(OpenPracticeSessionScheduled scheduled)
    {
        _sessions.GetOrAdd(scheduled.SessionId, id => new ListOpenPracticeSession
        {
            Id = scheduled.SessionId,
            Name = scheduled.Name,
            TrackId = scheduled.TrackId,
            TrackDetectorIds = scheduled.TrackDetectors
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

    private void When(OpenPracticeSessionLaneEnabled laneEnabled)
    {
        if (_sessions.TryGetValue(laneEnabled.SessionId, out var session))
        {
            var lane = session.Lanes.FirstOrDefault(l => l.Lane == laneEnabled.Lane);
            if (lane == null)
            {
                lane = new ListOpenPracticeSessionLane
                {
                    Lane = laneEnabled.Lane,
                };
                session.Lanes.Add(lane);
            }
            lane.IsEnabled = true;
        }
    }
    private void When(OpenPracticeSessionLaneDisabled laneDisabled)
    {
        if (_sessions.TryGetValue(laneDisabled.SessionId, out var session))
        {
            var lane = session.Lanes.FirstOrDefault(l => l.Lane == laneDisabled.Lane);
            if (lane == null)
            {
                lane = new ListOpenPracticeSessionLane
                {
                    Lane = laneDisabled.Lane,
                };
                session.Lanes.Add(lane);
            }
            lane.IsEnabled = false;
        }
    }
}
