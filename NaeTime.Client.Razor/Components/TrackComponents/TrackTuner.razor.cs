using Microsoft.AspNetCore.Components;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Client.Razor.Components.TrackComponents;
public partial class TrackTuner : ComponentBase
{
    [Inject]
    public ITrackQueryHandler TrackQueryHandler { get; set; } = default!;

    [EditorRequired]
    [Parameter]
    public Guid TrackId { get; set; }

    private Track? _track;
    private int _laneCount = 0;
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _track = await TrackQueryHandler.GetTrack(TrackId);

        if (_track == null)
        {
            return;
        }
    }
}
