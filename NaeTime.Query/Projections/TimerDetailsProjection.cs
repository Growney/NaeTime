using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class TimerDetailsProjection : ITimerDetailsProjection
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, TimerStatus>> _timerLaneStatus = new();
    private readonly ConcurrentDictionary<Guid, string> _timerName = new();

    private void When(ImmersionRCLapRFNetworkDeviceRegistered registered)
    {
        _timerName[registered.TimerId] = registered.Name;
    }
    private void When(ImmersionRCLapRFRenamed renamed)
    {
        _timerName[renamed.TimerId] = renamed.Name;
    }

    private TimerStatus AggregateStatuses(IEnumerable<TimerStatus> statuses)
    {
        TimerStatus finalStatus = TimerStatus.Unknown;

        foreach (TimerStatus status in statuses)
        {
            if (status != TimerStatus.Ok)
            {
                finalStatus = status;
                break;
            }
        }

        return finalStatus;
    }

    public TimerDetails GetDetails(Guid timerId)
    {
        throw new NotImplementedException();
    }

    public TimerLaneDetails GetLaneDetails(Guid timerId, byte laneId)
    {
        throw new NotImplementedException();
    }
}
