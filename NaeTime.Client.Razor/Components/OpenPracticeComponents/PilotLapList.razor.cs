using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class PilotLapList : ComponentBase
{
    [Parameter]
    public Guid PilotId { get; set; }
    [Parameter]
    public Guid SessionId { get; set; }
    [Parameter]
    public string? PilotName { get; set; }
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventRegistrarScope EventRegistrarScope { get; set; } = null!;

    private readonly List<OpenPracticeLap> _laps = new();

    protected override async Task OnInitializedAsync()
    {
        EventRegistrarScope.RegisterHub(this);

        IEnumerable<OpenPractice.Messages.Models.Lap>? initialLaps = await RpcClient.InvokeAsync<IEnumerable<NaeTime.OpenPractice.Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", SessionId, PilotId);

        if (initialLaps != null)
        {
            _laps.AddRange(initialLaps.Select(x => new OpenPracticeLap()
            {
                Id = x.Id,
                PilotId = x.PilotId,
                PilotName = null,
                StartedUtc = x.StartedUtc,
                FinishedUtc = x.FinishedUtc,
                Status = x.Status switch
                {
                    OpenPractice.Messages.Models.LapStatus.Invalid => OpenPracticeLapStatus.Invalid,
                    OpenPractice.Messages.Models.LapStatus.Completed => OpenPracticeLapStatus.Completed,
                    _ => throw new NotImplementedException()
                },
                TotalMilliseconds = x.TotalMilliseconds
            }));
        }

        await base.OnInitializedAsync();
    }

    public async Task When(OpenPracticeLapRemoved removed)
    {
        if (SessionId != removed.SessionId)
        {
            return;
        }

        if (PilotId != removed.PilotId)
        {
            return;
        }

        int lapIndex = _laps.FindIndex(x => x.Id == removed.LapId);

        if (lapIndex < 0)
        {
            return;
        }

        _laps.RemoveAt(lapIndex);

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapCompleted lap)
    {
        if (SessionId != lap.SessionId)
        {
            return;
        }

        if (PilotId != lap.PilotId)
        {
            return;
        }

        _laps.Add(new OpenPracticeLap()
        {
            Id = lap.LapId,
            PilotId = lap.PilotId,
            PilotName = null,
            StartedUtc = lap.StartedUtc,
            FinishedUtc = lap.FinishedUtc,
            TotalMilliseconds = lap.TotalMilliseconds,
            Status = OpenPracticeLapStatus.Completed
        });
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapInvalidated lap)
    {
        if (SessionId != lap.SessionId)
        {
            return;
        }

        if (PilotId != lap.PilotId)
        {
            return;
        }

        _laps.Add(new OpenPracticeLap()
        {
            Id = lap.LapId,
            PilotId = lap.PilotId,
            PilotName = null,
            StartedUtc = lap.StartedUtc,
            FinishedUtc = lap.FinishedUtc,
            TotalMilliseconds = lap.TotalMilliseconds,
            Status = OpenPracticeLapStatus.Invalid
        });

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapDisputed lap)
    {
        if (SessionId != lap.SessionId)
        {
            return;
        }

        if (PilotId != lap.PilotId)
        {
            return;
        }

        int lapIndex = _laps.FindIndex(x => x.Id == lap.LapId);

        if (lapIndex < 0)
        {
            return;
        }

        _laps[lapIndex].Status = lap.ActualStatus switch
        {
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid => OpenPracticeLapStatus.Invalid,
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed => OpenPracticeLapStatus.Completed,
            _ => throw new NotImplementedException()
        };


        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
}
