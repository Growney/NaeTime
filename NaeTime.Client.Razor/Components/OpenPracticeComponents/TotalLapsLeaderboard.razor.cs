using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class TotalLapsLeaderboard : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }
    private readonly List<TotalLapLeaderboardPosition> _positions = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    public async Task UpdatePosition(Guid sessionId, Guid pilotId, int? position, int totalLaps, DateTime firstLapCompletionUtc)
    {
        if (sessionId != SessionId)
        {
            return;
        }

        TotalLapLeaderboardPosition? existingPosition = _positions.FirstOrDefault(x => x.PilotId == pilotId);

        if (existingPosition == null)
        {
            existingPosition = new TotalLapLeaderboardPosition
            {
                PilotId = pilotId,
            };
            _positions.Add(existingPosition);
        }

        if (position.HasValue)
        {
            existingPosition.Position = position.Value;
        }

        existingPosition.TotalLaps = totalLaps;
        existingPosition.FirstLapCompletionUtc = firstLapCompletionUtc;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private string GetPilotName(Guid pilotId) => _pilots.FirstOrDefault(x => x.Id == pilotId)?.CallSign ?? "Unknown";
}
