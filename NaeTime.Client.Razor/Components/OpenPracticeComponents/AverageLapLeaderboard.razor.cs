using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class AverageLapLeaderboard
{
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }
    [Inject]
    private INaeTimePersistence Persistence { get; set; } = null!;
    [Inject]
    private IEventRegistrarScope EventRegistrarScope { get; set; } = null!;

    private readonly List<AverageLapLeaderboardPosition> _positions = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        EventRegistrarScope.RegisterHub(this);

        IEnumerable<Persistence.Abstractions.OpenPractice.AverageLapLeaderboardPosition> initialPositions = await Persistence.OpenPractice.GetOpenPracticeSessionAverageLapLeaderboardPositions(SessionId);

        _positions.AddRange(initialPositions.Select(position => new AverageLapLeaderboardPosition()
        {
            Position = position.Position,
            PilotId = position.PilotId,
            AverageMilliseconds = position.AverageMilliseconds,
            FirstLapCompletionUtc = position.FirstLapCompletion,
        }));


        IEnumerable<Persistence.Abstractions.Management.Pilot> initialPilots = await Persistence.Management.GetPilots();

        _pilots.AddRange(initialPilots.Select(x => new Pilot()
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            CallSign = x.CallSign
        }));


        await base.OnInitializedAsync();
    }
    public async Task UpdatePosition(Guid sessionId, Guid pilotId, int? position, double averageMilliseconds, DateTime firstLapCompletionUtc)
    {
        if (sessionId != SessionId)
        {
            return;
        }

        AverageLapLeaderboardPosition? existingPosition = _positions.FirstOrDefault(x => x.PilotId == pilotId);

        if (existingPosition == null)
        {
            existingPosition = new AverageLapLeaderboardPosition
            {
                PilotId = pilotId,
            };
            _positions.Add(existingPosition);
        }

        if (position.HasValue)
        {
            existingPosition.Position = position.Value;
        }

        existingPosition.AverageMilliseconds = averageMilliseconds;
        existingPosition.FirstLapCompletionUtc = firstLapCompletionUtc;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public Task When(AverageLapLeaderboardPositionReduced reduced) => UpdatePosition(reduced.SessionId, reduced.PilotId, reduced.NewPosition, reduced.AverageMilliseconds, reduced.FirstLapCompletionUtc);
    public Task When(AverageLapLeaderboardPositionImproved improved) => UpdatePosition(improved.SessionId, improved.PilotId, improved.NewPosition, improved.AverageMilliseconds, improved.FirstLapCompletionUtc);
    public Task When(AverageLapLeaderboardRecordImproved improved) => UpdatePosition(improved.SessionId, improved.PilotId, null, improved.AverageMilliseconds, improved.FirstLapCompletionUtc);
    public Task When(AverageLapLeaderboardRecordReduced reduced) => UpdatePosition(reduced.SessionId, reduced.PilotId, null, reduced.AverageMilliseconds, reduced.FirstLapCompletionUtc);
    public async Task When(AverageLapLeaderboardPositionRemoved removed)
    {
        if (removed.SessionId != SessionId)
        {
            return;
        }

        AverageLapLeaderboardPosition? existingPosition = _positions.FirstOrDefault(x => x.PilotId == removed.PilotId);

        if (existingPosition != null)
        {
            _positions.Remove(existingPosition);
        }

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public void Dispose() => EventRegistrarScope?.Dispose();

    private string GetPilotName(Guid pilotId) => _pilots.FirstOrDefault(x => x.Id == pilotId)?.CallSign ?? "Unknown";
}