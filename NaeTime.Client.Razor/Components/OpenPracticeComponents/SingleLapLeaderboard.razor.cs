using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SingleLapLeaderboard : ComponentBase
{
    [Parameter]
    public Guid SessionId { get; set; }

    private readonly List<SingleLapLeaderboardPosition> _positions = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    public async Task UpdatePosition(Guid sessionId, Guid pilotId, int? position, long totalMilliseconds, DateTime lastLapCompletionUtc, Guid lapId)
    {
        if (sessionId != SessionId)
        {
            return;
        }

        SingleLapLeaderboardPosition? existingPosition = _positions.FirstOrDefault(x => x.PilotId == pilotId);

        if (existingPosition == null)
        {
            existingPosition = new SingleLapLeaderboardPosition
            {
                PilotId = pilotId,
            };
            _positions.Add(existingPosition);
        }

        if (position.HasValue)
        {
            existingPosition.Position = position.Value;
        }

        existingPosition.TotalMilliseconds = totalMilliseconds;
        existingPosition.CompletionUtc = lastLapCompletionUtc;
        existingPosition.LapId = lapId;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private string GetPilotName(Guid pilotId) => _pilots.FirstOrDefault(x => x.Id == pilotId)?.CallSign ?? "Unknown";
    public Task Invalidate(Guid lapId, Guid pilotId) => Task.CompletedTask;
}
