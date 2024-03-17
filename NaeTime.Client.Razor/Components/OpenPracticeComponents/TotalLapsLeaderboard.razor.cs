using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Management.Messages.Requests;
using NaeTime.Management.Messages.Responses;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class TotalLapsLeaderboard : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }
    [Parameter]
    public uint LapCap { get; set; }
    [Inject]
    public IPublishSubscribe PublishSubscribe { get; set; } = null!;

    private readonly List<TotalLapLeaderboardPosition> _positions = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        PublishSubscribe.Subscribe<TotalLapsLeaderboardPositionReduced>(this, When);
        PublishSubscribe.Subscribe<TotalLapsLeaderboardPositionImproved>(this, When);
        PublishSubscribe.Subscribe<TotalLapsLeaderboardRecordImproved>(this, When);
        PublishSubscribe.Subscribe<TotalLapsLeaderboardRecordReduced>(this, When);
        PublishSubscribe.Subscribe<TotalLapsLeaderboardPositionRemoved>(this, When);

        var initialPositions = await PublishSubscribe.Request<TotalLapsLeaderboardRequest, TotalLapsLeaderboardResponse>(new TotalLapsLeaderboardRequest(SessionId));

        if (initialPositions != null)
        {
            foreach (var position in initialPositions.Positions)
            {
                _positions.Add(new TotalLapLeaderboardPosition()
                {
                    Position = position.Position,
                    PilotId = position.PilotId,
                    TotalLaps = position.TotalLaps,
                    FirstLapCompletionUtc = position.FirstLapCompletionUtc,
                });
            }
        }

        var initialPilots = await PublishSubscribe.Request<PilotsRequest, PilotsResponse>();

        if (initialPilots != null)
        {
            _pilots.AddRange(initialPilots.Pilots.Select(x => new Pilot()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                CallSign = x.CallSign
            }));
        }

        await base.OnInitializedAsync();
    }
    public async Task UpdatePosition(Guid sessionId, Guid pilotId, int? position, int totalLaps, DateTime firstLapCompletionUtc)
    {
        if (sessionId != SessionId)
        {
            return;
        }

        var existingPosition = _positions.FirstOrDefault(x => x.PilotId == pilotId);

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
    public Task When(TotalLapsLeaderboardPositionReduced reduced) => UpdatePosition(reduced.SessionId, reduced.PilotId, reduced.NewPosition, reduced.TotalLaps, reduced.FirstLapCompletionUtc);
    public Task When(TotalLapsLeaderboardPositionImproved improved) => UpdatePosition(improved.SessionId, improved.PilotId, improved.NewPosition, improved.TotalLaps, improved.FirstLapCompletionUtc);
    public Task When(TotalLapsLeaderboardRecordImproved improved) => UpdatePosition(improved.SessionId, improved.PilotId, null, improved.TotalLaps, improved.FirstLapCompletionUtc);
    public Task When(TotalLapsLeaderboardRecordReduced reduced) => UpdatePosition(reduced.SessionId, reduced.PilotId, null, reduced.TotalLaps, reduced.FirstLapCompletionUtc);
    public async Task When(TotalLapsLeaderboardPositionRemoved removed)
    {
        if (removed.SessionId != SessionId)
        {
            return;
        }

        var existingPosition = _positions.FirstOrDefault(x => x.PilotId == removed.PilotId);

        if (existingPosition != null)
        {
            _positions.Remove(existingPosition);
        }

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public void Dispose() => PublishSubscribe.Unsubscribe(this);

    private string GetPilotName(Guid pilotId) => _pilots.FirstOrDefault(x => x.Id == pilotId)?.CallSign ?? "Unknown";
}
