using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Persistence.Abstractions.Timing;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class OpenPracticeTotalLapsLeaderboardManager : LeaderboardManager<TotalLapRecord>
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimeOrchestrator _orchestrator;

    public OpenPracticeTotalLapsLeaderboardManager(IEventClient eventClient, INaeTimeOrchestrator orchestrator)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }
    public async Task When(OpenPracticeLapCompleted completed)
    {
        IEnumerable<Lap>? pilotLaps = await _orchestrator.Timing.GetPilotOpenPracticeSessionLaps(completed.SessionId, completed.PilotId);

        IEnumerable<Lap>? validLaps = pilotLaps?.Where(x => x.Status == LapStatus.Valid && x.Id != completed.LapId);
        int lapCount = validLaps?.Count() ?? 0;

        lapCount++;

        Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime firstLapCompletionUtc = firstLap?.ExitDetection?.UtcTime ?? completed.StartedUtc;

        await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new TotalLapRecord(lapCount, firstLapCompletionUtc));
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        IEnumerable<Lap>? pilotLaps = await _orchestrator.Timing.GetPilotOpenPracticeSessionLaps(disputed.SessionId, disputed.PilotId);

        Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
        IEnumerable<Lap>? validLaps = pilotLaps?.Where(x => x.Status == LapStatus.Valid
        && (x.Id != disputed.LapId || disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Valid));
        int lapCount = validLaps?.Count() ?? 0;

        Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.ExitDetection?.UtcTime ?? lap?.EntryDetection.UtcTime;

        if (firstLapCompletionUtc == null)
        {
            await HandleUpdatedRecord(disputed.SessionId, disputed.PilotId, null);
        }
        else
        {
            await HandleUpdatedRecord(disputed.SessionId, disputed.PilotId, new TotalLapRecord(lapCount, firstLapCompletionUtc.Value));
        }
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        IEnumerable<Lap>? pilotLaps = await _orchestrator.Timing.GetPilotOpenPracticeSessionLaps(removed.SessionId, removed.PilotId);

        Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == removed.LapId);
        IEnumerable<Lap>? validLaps = pilotLaps?.Where(x => x.Status == LapStatus.Valid && x.Id != lap?.Id);
        int lapCount = validLaps?.Count() ?? 0;

        Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.ExitDetection?.UtcTime ?? lap?.EntryDetection?.UtcTime;

        if (firstLapCompletionUtc == null)
        {
            await HandleUpdatedRecord(removed.SessionId, removed.PilotId, null);
        }
        else
        {
            await HandleUpdatedRecord(removed.SessionId, removed.PilotId, new TotalLapRecord(lapCount, firstLapCompletionUtc.Value));
        }
    }

    protected override async Task<IEnumerable<LeaderboardPosition<TotalLapRecord>>> GetExistingPositions(Guid sessionId)
    {
        IEnumerable<Persistence.Abstractions.OpenPractice.TotalLapLeaderboardPosition>? response = await _orchestrator.OpenPractice.GetOpenPracticeSessionTotalLapLeaderboardPositions(sessionId);

        return response?.Select(x => new LeaderboardPosition<TotalLapRecord>(x.PilotId, x.Position, new TotalLapRecord(x.TotalLaps, x.FirstLapCompletionUtc))) ?? Enumerable.Empty<LeaderboardPosition<TotalLapRecord>>();
    }
    protected override Task OnPositionImproved(Guid sessionId, Guid pilotId, int newPosition, int? oldPosition, TotalLapRecord newRecord)
        => _eventClient.PublishAsync(new TotalLapsLeaderboardPositionImproved(sessionId, newPosition, oldPosition, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionReduced(Guid sessionId, Guid pilotId, int newPosition, int oldPosition, TotalLapRecord newRecord)
        => _eventClient.PublishAsync(new TotalLapsLeaderboardPositionReduced(sessionId, newPosition, oldPosition, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionRemoved(Guid sessionId, Guid pilotId)
        => _eventClient.PublishAsync(new TotalLapsLeaderboardPositionRemoved(sessionId, pilotId));
    protected override Task OnRecordImproved(Guid sessionId, Guid pilotId, TotalLapRecord newRecord)
        => _eventClient.PublishAsync(new TotalLapsLeaderboardRecordImproved(sessionId, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnRecordReduced(Guid sessionId, Guid pilotId, TotalLapRecord newRecord)
        => _eventClient.PublishAsync(new TotalLapsLeaderboardRecordReduced(sessionId, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
}