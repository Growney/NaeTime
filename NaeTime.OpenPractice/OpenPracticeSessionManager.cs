﻿using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.OpenPractice;
public class OpenPracticeSessionManager
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public OpenPracticeSessionManager(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(LapCompleted lapCompleted)
    {
        Messages.Models.OpenPracticeSession? sessionResponse = await _rpcClient.InvokeAsync<Messages.Models.OpenPracticeSession?>("GetOpenPracticeSession", lapCompleted.SessionId);

        if (sessionResponse == null)
        {
            return;
        }

        Messages.Models.PilotLane? pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapCompleted.Lane);
        if (pilotLane == null)
        {
            return;
        }

        Guid newLapId = Guid.NewGuid();

        await _eventClient.PublishAsync(new OpenPracticeLapCompleted(Guid.NewGuid(), lapCompleted.SessionId, pilotLane.PilotId, lapCompleted.StartedUtcTime, lapCompleted.FinishedUtcTime, lapCompleted.TotalTime)).ConfigureAwait(false);
    }
    public async Task When(LapInvalidated lapInvalidated)
    {
        Messages.Models.OpenPracticeSession? sessionResponse = await _rpcClient.InvokeAsync<Messages.Models.OpenPracticeSession?>("GetOpenPracticeSession", lapInvalidated.SessionId);

        if (sessionResponse == null)
        {
            return;
        }

        Messages.Models.PilotLane? pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapInvalidated.Lane);
        if (pilotLane == null)
        {
            return;
        }

        await _eventClient.PublishAsync(new OpenPracticeLapInvalidated(Guid.NewGuid(), lapInvalidated.SessionId, pilotLane.PilotId, lapInvalidated.LapNumber, lapInvalidated.StartedUtcTime, lapInvalidated.FinishedUtcTime, lapInvalidated.TotalTime));
    }
}
