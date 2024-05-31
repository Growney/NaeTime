﻿using NaeTime.Hardware.Abstractions;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.OpenPractice;
internal class DetectionService
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;
    private readonly ISoftwareTimer _softwareTimer;

    public DetectionService(IEventClient eventClient, IRemoteProcedureCallClient rpcClient, ISoftwareTimer softwareTimer)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public async Task When(OpenPracticeSessionDetectionTriggered triggered)
    {
        Messages.Models.OpenPracticeSession? session = await _rpcClient.InvokeAsync<Messages.Models.OpenPracticeSession>("GetOpenPracticeSession", triggered.SessionId);

        if (session == null)
        {
            return;
        }

        await _eventClient.PublishAsync(new SessionDetectionOccured(session.Id, triggered.TimerId, triggered.Lane, session.TrackId, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, null, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeSessionInvalidationTriggered triggered)
    {
        Messages.Models.OpenPracticeSession? session = await _rpcClient.InvokeAsync<Messages.Models.OpenPracticeSession>("GetOpenPracticeSession", triggered.SessionId);

        if (session == null)
        {
            return;
        }

        Timing.Messages.Models.LaneActiveTimings? activeTimings = await _rpcClient.InvokeAsync<Timing.Messages.Models.LaneActiveTimings>("GetLaneActiveTimings", triggered.SessionId, triggered.Lane);


        if (activeTimings == null)
        {
            return;
        }

        if (activeTimings.Lap == null)
        {
            return;
        }

        long finishedSoftwareTime = _softwareTimer.ElapsedMilliseconds;
        DateTime finishedUtcTime = DateTime.UtcNow;

        long totalTime = CalculateTotalTime(activeTimings.Lap.StartedSoftwareTime, activeTimings.Lap.StartedUtcTime, finishedSoftwareTime, finishedUtcTime);

        await _eventClient.PublishAsync(new LapInvalidated(session.Id, triggered.Lane, activeTimings.LapNumber, activeTimings.Lap.StartedSoftwareTime, activeTimings.Lap.StartedUtcTime, activeTimings.Lap.StartedHardwareTime, finishedSoftwareTime, finishedUtcTime, null, totalTime, LapInvalidated.LapInvalidReason.Cancelled)).ConfigureAwait(false);
    }
    private long CalculateTotalTime(long startSoftwareTime, DateTime startUtcTime, long endSoftwareTime, DateTime endUtcTime)
    {
        long softwareDifference = endSoftwareTime - startSoftwareTime;
        if (softwareDifference < 0)
        {
            return (long)endUtcTime.Subtract(startUtcTime).TotalMilliseconds;
        }
        else
        {
            return softwareDifference;
        }
    }
    public async Task When(ActiveOpenPracticeSessionDetectionOccured detection)
    {
        Messages.Models.OpenPracticeSession? session = await _rpcClient.InvokeAsync<Messages.Models.OpenPracticeSession>("GetOpenPracticeSession", detection.SessionId);

        if (session == null)
        {
            return;
        }

        await _eventClient.PublishAsync(new SessionDetectionOccured(session.Id, detection.TimerId, detection.Lane, session.TrackId, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
    }
}
