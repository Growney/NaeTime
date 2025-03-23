using Microsoft.AspNetCore.Components;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Client.Razor.Components.TrackComponents;
public partial class TrackTuner : ComponentBase
{
    [Inject]
    public INaeTimePersistence Persistence { get; set; } = default!;

    [EditorRequired]
    [Parameter]
    public Guid TrackId { get; set; }

    private readonly List<NaeTime.Persistence.Abstractions.Hardware.TimerDetails> _trackTimers = new();
    private int _laneCount = 0;
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Persistence.Abstractions.Management.Track? track = await Persistence.Management.GetTrack();

        if (track == null)
        {
            return;
        }

        IEnumerable<NaeTime.Persistence.Abstractions.Hardware.TimerDetails>? allTimers = await Persistence.Hardware.GetAllTimerDetails();

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
            NaeTime.Persistence.Abstractions.Hardware.TimerDetails? timer = allTimers.FirstOrDefault(x => x.Id == timerId);
            if (timer != null)
            {
                _laneCount = Math.Min(_laneCount, timer.MaxLanes);
                _trackTimers.Add(timer);
            }
        }
    }
}
