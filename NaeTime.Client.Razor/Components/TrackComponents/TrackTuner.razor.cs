using Microsoft.AspNetCore.Components;
using NaeTime.Orchestrator.Abstractions;

namespace NaeTime.Client.Razor.Components.TrackComponents;
public partial class TrackTuner : ComponentBase
{
    [Inject]
    public INaeTimeOrchestrator Orchestrator { get; set; } = default!;

    [EditorRequired]
    [Parameter]
    public Guid TrackId { get; set; }

    private readonly List<NaeTime.Persistence.Abstractions.Hardware.TimerDetails> _trackTimers = new();
    private int _laneCount = 0;
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _trackTimers.Clear();
        _trackTimers.AddRange(await Orchestrator.Management.GetTrackTimers(TrackId));
        _laneCount = await Orchestrator.Management.GetTrackAllowedLanes(TrackId);
    }
}
