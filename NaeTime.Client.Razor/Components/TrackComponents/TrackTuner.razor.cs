using Microsoft.AspNetCore.Components;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.TrackComponents;
public partial class TrackTuner : ComponentBase
{
    [Inject]
    public IRemoteProcedureCallClient RpcClient { get; set; } = default!;

    [EditorRequired]
    [Parameter]
    public Guid TrackId { get; set; }

    private readonly List<Hardware.Messages.Models.TimerDetails> _trackTimers = new();
    private int _laneCount = 0;
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Management.Messages.Models.Track? track = await RpcClient.InvokeAsync<Management.Messages.Models.Track>("GetTrack", TrackId);

        if (track == null)
        {
            return;
        }

        IEnumerable<Hardware.Messages.Models.TimerDetails>? allTimers = await RpcClient.InvokeAsync<IEnumerable<Hardware.Messages.Models.TimerDetails>>("GetAllTimerDetails");

        if (allTimers == null)
        {
            return;
        }

        if (allTimers.Any())
        {
            _laneCount = 8;
        }

        _laneCount = 8;

        foreach (Guid timerId in track.Timers)
        {
            Hardware.Messages.Models.TimerDetails? timer = allTimers.FirstOrDefault(x => x.Id == timerId);
            if (timer != null)
            {
                _laneCount = Math.Min(_laneCount, timer.MaxLanes);
                _trackTimers.Add(timer);
            }
        }
    }
}
