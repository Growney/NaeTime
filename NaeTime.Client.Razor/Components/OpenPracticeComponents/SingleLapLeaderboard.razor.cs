using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SingleLapLeaderboard : ComponentBase, IDisposable
{
    [Parameter]
    public Guid SessionId { get; set; }
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventRegistrarScope EventRegistrarScope { get; set; } = null!;

    private readonly List<SingleLapLeaderboardPosition> _positions = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        EventRegistrarScope.RegisterHub(this);

        IEnumerable<OpenPractice.Messages.Models.SingleLapLeaderboardPosition>? initialPositions = await RpcClient.InvokeAsync<IEnumerable<OpenPractice.Messages.Models.SingleLapLeaderboardPosition>>("GetOpenPracticeSessionSingleLapLeaderboardPositions", SessionId);

        if (initialPositions != null)
        {
            foreach (OpenPractice.Messages.Models.SingleLapLeaderboardPosition position in initialPositions)
            {
                _positions.Add(new SingleLapLeaderboardPosition()
                {
                    Position = position.Position,
                    PilotId = position.PilotId,
                    TotalMilliseconds = position.TotalMilliseconds,
                    CompletionUtc = position.CompletionUtc,
                    LapId = position.LapId
                });
            }
        }

        IEnumerable<Management.Messages.Models.Pilot>? initialPilots = await RpcClient.InvokeAsync<IEnumerable<Management.Messages.Models.Pilot>>("GetPilots");

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
    public Task When(SingleLapLeaderboardPositionReduced reduced) => UpdatePosition(reduced.SessionId, reduced.PilotId, reduced.NewPosition, reduced.TotalMilliseconds, reduced.CompletionUtc, reduced.LapId);
    public Task When(SingleLapLeaderboardPositionImproved improved) => UpdatePosition(improved.SessionId, improved.PilotId, improved.NewPosition, improved.TotalMilliseconds, improved.CompletionUtc, improved.LapId);
    public Task When(SingleLapLeaderboardRecordImproved improved) => UpdatePosition(improved.SessionId, improved.PilotId, null, improved.TotalMilliseconds, improved.CompletionUtc, improved.LapId);
    public Task When(SingleLapLeaderboardRecordReduced reduced) => UpdatePosition(reduced.SessionId, reduced.PilotId, null, reduced.TotalMilliseconds, reduced.CompletionUtc, reduced.LapId);
    public async Task When(SingleLapLeaderboardPositionRemoved removed)
    {
        if (removed.SessionId != SessionId)
        {
            return;
        }

        SingleLapLeaderboardPosition? existingPosition = _positions.FirstOrDefault(x => x.PilotId == removed.PilotId);

        if (existingPosition != null)
        {
            _positions.Remove(existingPosition);
        }

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private string GetPilotName(Guid pilotId) => _pilots.FirstOrDefault(x => x.Id == pilotId)?.CallSign ?? "Unknown";

    public void Dispose() => EventRegistrarScope?.Dispose();
}
