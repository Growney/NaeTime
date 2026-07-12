using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class TimerDetailsProjection : ITimerDetailsProjection
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, LaneStatus>> _timerLaneStatus = new();
    private readonly ConcurrentDictionary<Guid, string> _timerName = new();
    private readonly ConcurrentDictionary<Guid, bool> _timerConnected = new();

    private void When(TimerRenamed registered)
    {
        _timerName[registered.TimerId] = registered.NewName;
    }
    private void When(TimerConnected connected)
    {
        _timerConnected[connected.TimerId] = true;
    }
    private void When(TimerDisconnected disconnected)
    {
        _timerConnected[disconnected.TimerId] = false;
    }
    private void When(TimerLaneMismatch statusUpdated)
    {
        ConcurrentDictionary<byte, LaneStatus> laneStatuses = _timerLaneStatus.GetOrAdd(statusUpdated.TimerId, new ConcurrentDictionary<byte, LaneStatus>());
        laneStatuses[statusUpdated.LaneId] = LaneStatus.Mismatched;
    }
    private void When(TimerLanePending pending)
    {
        ConcurrentDictionary<byte, LaneStatus> laneStatuses = _timerLaneStatus.GetOrAdd(pending.TimerId, new ConcurrentDictionary<byte, LaneStatus>());
        laneStatuses[pending.LaneId] = LaneStatus.Pending;
    }
    private void When(TimerLaneConfirmed confirmed)
    {
        ConcurrentDictionary<byte, LaneStatus> laneStatuses = _timerLaneStatus.GetOrAdd(confirmed.TimerId, new ConcurrentDictionary<byte, LaneStatus>());
        laneStatuses[confirmed.LaneId] = LaneStatus.Ok;
    }
    public TimerDetails GetDetails(Guid timerId)
    {
        if (!_timerLaneStatus.TryGetValue(timerId, out var laneStatuses))
        {
            laneStatuses = new ConcurrentDictionary<byte, LaneStatus>();
        }
        string timerName = _timerName.TryGetValue(timerId, out var name) ? name : "Unknown Timer";
        List<TimerLaneDetails> laneDetails = laneStatuses
            .Select(kvp => new TimerLaneDetails(kvp.Key, kvp.Value))
            .ToList();

        if(!_timerConnected.TryGetValue(timerId, out var isConnected))
        {
            isConnected = false;
        }
        return new TimerDetails(timerId, timerName, isConnected, laneDetails);
    }

    public TimerLaneDetails GetLaneDetails(Guid timerId, byte laneId)
    {
        if (!_timerLaneStatus.TryGetValue(timerId, out var laneStatuses) || !laneStatuses.TryGetValue(laneId, out var status))
        {
            status = LaneStatus.Unknown;
        }
        return new TimerLaneDetails(laneId, status);
    }
}
