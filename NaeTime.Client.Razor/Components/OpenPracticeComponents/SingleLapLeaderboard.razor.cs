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
public partial class SingleLapLeaderboard : ComponentBase, IDisposable
{
    [Parameter]
    public Guid SessionId { get; set; }
    [Inject]
    public IPublishSubscribe PublishSubscribe { get; set; } = null!;

    private readonly List<SingleLapLeaderboardPosition> _positions = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        PublishSubscribe.Subscribe<SingleLapLeaderboardPositionReduced>(this, When);
        PublishSubscribe.Subscribe<SingleLapLeaderboardPositionImproved>(this, When);
        PublishSubscribe.Subscribe<SingleLapLeaderboardRecordImproved>(this, When);
        PublishSubscribe.Subscribe<SingleLapLeaderboardRecordReduced>(this, When);
        PublishSubscribe.Subscribe<SingleLapLeaderboardPositionRemoved>(this, When);

        var initialPositions = await PublishSubscribe.Request<SingleLapLeaderboardRequest, SingleLapLeaderboardResponse>(new SingleLapLeaderboardRequest(SessionId));

        if (initialPositions != null)
        {
            foreach (var position in initialPositions.Positions)
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

    public async Task UpdatePosition(Guid sessionId, Guid pilotId, int? position, long totalMilliseconds, DateTime lastLapCompletionUtc, Guid lapId)
    {
        if (sessionId != SessionId)
        {
            return;
        }

        var existingPosition = _positions.FirstOrDefault(x => x.PilotId == pilotId);

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

        var existingPosition = _positions.FirstOrDefault(x => x.PilotId == removed.PilotId);

        if (existingPosition != null)
        {
            _positions.Remove(existingPosition);
        }

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private string GetPilotName(Guid pilotId) => _pilots.FirstOrDefault(x => x.Id == pilotId)?.CallSign ?? "Unknown";

    public void Dispose() => PublishSubscribe.Unsubscribe(this);
}
