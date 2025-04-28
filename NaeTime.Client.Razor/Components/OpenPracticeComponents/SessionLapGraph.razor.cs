using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Persistence.Abstractions.Timing.Extensions;
using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SessionLapGraph
{
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }

    [Inject]
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;
    [Inject]
    private IEventRegistrarScope EventRegistrarScope { get; set; } = null!;

    private class LapTime
    {
        public Guid LapId { get; set; }
        public int LapNumber { get; set; }
        public double TotalSeconds { get; set; }
        public DateTime LapCompleted { get; set; }

    }

    private readonly ConcurrentDictionary<Guid, List<LapTime>> _lapTimes = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        EventRegistrarScope.RegisterHub(this);

        IEnumerable<Persistence.Abstractions.Management.Pilot> initialPilots = await Orchestrator.Management.GetPilots();

        if (initialPilots != null)
        {
            _pilots.AddRange(initialPilots.Select(x => new Pilot()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                CallSign = x.CallSign
            }));
        }

        IEnumerable<Persistence.Abstractions.Timing.Lap> sessionLaps = await Orchestrator.Timing.GetOpenPracticeSessionLaps(SessionId);

        foreach (Persistence.Abstractions.Timing.Lap lap in sessionLaps)
        {
            if (lap.Status != Persistence.Abstractions.Timing.LapStatus.Valid)
            {
                continue;
            }

            List<LapTime> pilotLapTimes = _lapTimes.GetOrAdd(lap.PilotId, _ => new List<LapTime>());

            pilotLapTimes.Add(new LapTime
            {
                LapId = lap.Id,
                LapNumber = pilotLapTimes.Count + 1,
                TotalSeconds = IDetectionExtensions.TimeBetween(lap.EntryDetection, lap.ExitDetection).TotalSeconds,
                LapCompleted = lap.ExitDetection?.UtcTime ?? lap.EntryDetection.UtcTime
            });
        }

        foreach (List<LapTime> pilotLapTimes in _lapTimes.Values)
        {
            SortAndMarkLapNumber(pilotLapTimes);
        }
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        if (SessionId != removed.SessionId)
        {
            return;
        }

        List<LapTime> pilotLapTimes = _lapTimes.GetOrAdd(removed.PilotId, _ => new List<LapTime>());

        int lapIndex = pilotLapTimes.FindIndex(x => x.LapId == removed.LapId);

        if (lapIndex < 0)
        {
            return;
        }

        pilotLapTimes.RemoveAt(lapIndex);

        SortAndMarkLapNumber(pilotLapTimes);
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapCompleted lap)
    {
        if (SessionId != lap.SessionId)
        {
            return;
        }

        List<LapTime> pilotLapTimes = _lapTimes.GetOrAdd(lap.PilotId, _ => new List<LapTime>());

        pilotLapTimes.Add(new LapTime()
        {
            LapId = lap.LapId,
            TotalSeconds = lap.TotalMilliseconds / 1000,
            LapCompleted = lap.FinishedUtc
        });

        SortAndMarkLapNumber(pilotLapTimes);
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapInvalidated lap)
    {
        if (SessionId != lap.SessionId)
        {
            return;
        }

        List<LapTime> pilotLapTimes = _lapTimes.GetOrAdd(lap.PilotId, _ => new List<LapTime>());

        int lapIndex = pilotLapTimes.FindIndex(x => x.LapId == lap.LapId);

        if (lapIndex < 0)
        {
            return;
        }

        pilotLapTimes.RemoveAt(lapIndex);

        SortAndMarkLapNumber(pilotLapTimes);
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapDisputed lap)
    {
        if (lap.SessionId != SessionId)
        {
            return;
        }

        List<LapTime> pilotLapTimes = _lapTimes.GetOrAdd(lap.PilotId, _ => new List<LapTime>());

        if (lap.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid)
        {
            int lapIndex = pilotLapTimes.FindIndex(x => x.LapId == lap.LapId);

            if (lapIndex < 0)
            {
                return;
            }

            pilotLapTimes.RemoveAt(lapIndex);
        }
        else
        {
            Persistence.Abstractions.Timing.Lap? sessionLap = await Orchestrator.Timing.GetOpenPracticeSessionLap(lap.LapId);

            if (sessionLap == null)
            {
                return;
            }

            pilotLapTimes.Add(new LapTime()
            {
                LapId = sessionLap.Id,
                TotalSeconds = IDetectionExtensions.TimeBetween(sessionLap.EntryDetection, sessionLap.ExitDetection).TotalSeconds,
                LapCompleted = sessionLap.ExitDetection?.UtcTime ?? sessionLap.EntryDetection.UtcTime
            });

        }

        SortAndMarkLapNumber(pilotLapTimes);
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private void SortAndMarkLapNumber(List<LapTime> times)
    {
        times.Sort((x, y) => x.LapCompleted.CompareTo(y.LapCompleted));

        for (int i = 0; i < times.Count; i++)
        {
            LapTime time = times[i];
            time.LapNumber = i + 1;
        }
    }
}