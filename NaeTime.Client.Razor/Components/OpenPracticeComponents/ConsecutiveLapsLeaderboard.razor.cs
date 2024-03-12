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
public partial class ConsecutiveLapsLeaderboard : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }
    [Parameter]
    public uint LapCap { get; set; }
    [Inject]
    public IPublishSubscribe PublishSubscribe { get; set; } = null!;

    private readonly List<ConsecutiveLapLeadboardPosition> _positions = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        PublishSubscribe.Subscribe<ConsecutiveLapLeaderboardPositionReduced>(this, When);
        PublishSubscribe.Subscribe<ConsecutiveLapLeaderboardPositionImproved>(this, When);
        PublishSubscribe.Subscribe<ConsecutiveLapLeaderboardRecordImproved>(this, When);
        PublishSubscribe.Subscribe<ConsecutiveLapLeaderboardRecordReduced>(this, When);
        PublishSubscribe.Subscribe<ConsecutiveLapLeaderboardPositionRemoved>(this, When);

        var initialPositions = await PublishSubscribe.Request<ConsecutiveLapLeaderboardRequest, ConsecutiveLapLeaderboardReponse>(new ConsecutiveLapLeaderboardRequest(SessionId, LapCap));

        if (initialPositions != null)
        {
            foreach (var position in initialPositions.Positions)
            {
                _positions.Add(new ConsecutiveLapLeadboardPosition()
                {
                    Position = position.Position,
                    PilotId = position.PilotId,
                    TotalLaps = position.TotalLaps,
                    TotalMilliseconds = position.TotalMilliseconds,
                    LastLapCompletionUtc = position.LastLapCompletionUtc,
                    IncludedLaps = position.IncludedLaps
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
    public async Task UpdatePosition(Guid sessionId, uint lapCap, Guid pilotId, int? position, uint totalLaps, long totalMilliseconds, DateTime lastLapCompletionUtc, IEnumerable<Guid> includedLaps)
    {
        if (lapCap != LapCap || sessionId != SessionId)
        {
            return;
        }

        var existingPosition = _positions.FirstOrDefault(x => x.PilotId == pilotId);

        if (existingPosition == null)
        {
            existingPosition = new ConsecutiveLapLeadboardPosition
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
        existingPosition.TotalMilliseconds = totalMilliseconds;
        existingPosition.LastLapCompletionUtc = lastLapCompletionUtc;
        existingPosition.IncludedLaps = includedLaps;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public Task When(ConsecutiveLapLeaderboardPositionReduced reduced) => UpdatePosition(reduced.SessionId, reduced.LapCap, reduced.PilotId, reduced.NewPosition, reduced.TotalLaps, reduced.TotalMilliseconds, reduced.LastLapCompletionUtc, reduced.IncludedLaps);
    public Task When(ConsecutiveLapLeaderboardPositionImproved improved) => UpdatePosition(improved.SessionId, improved.LapCap, improved.PilotId, improved.NewPosition, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps);
    public Task When(ConsecutiveLapLeaderboardRecordImproved improved) => UpdatePosition(improved.SessionId, improved.LapCap, improved.PilotId, null, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps);
    public Task When(ConsecutiveLapLeaderboardRecordReduced reduced) => UpdatePosition(reduced.SessionId, reduced.LapCap, reduced.PilotId, null, reduced.TotalLaps, reduced.TotalMilliseconds, reduced.LastLapCompletionUtc, reduced.IncludedLaps);
    public async Task When(ConsecutiveLapLeaderboardPositionRemoved removed)
    {
        if (removed.LapCap != LapCap || removed.SessionId != SessionId)
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
