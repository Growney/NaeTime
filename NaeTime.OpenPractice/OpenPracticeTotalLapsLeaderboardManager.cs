using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class OpenPracticeTotalLapsLeaderboardManager : LeaderboardManager<TotalLapRecord>
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimePersistence _persistence;

    public OpenPracticeTotalLapsLeaderboardManager(IEventClient eventClient, INaeTimePersistence persistence)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }
    public async Task When(OpenPracticeLapCompleted completed)
    {
        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? pilotLaps = await _persistence.OpenPractice.GetPilotOpenPracticeSessionLaps(completed.SessionId, completed.PilotId);

        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Persistence.Abstractions.OpenPractice.LapStatus.Completed && x.Id != completed.LapId);
        int lapCount = validLaps?.Count() ?? 0;

        lapCount++;

        Persistence.Abstractions.OpenPractice.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime firstLapCompletionUtc = firstLap?.FinishedUtc ?? completed.StartedUtc;

        await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new TotalLapRecord(lapCount, firstLapCompletionUtc));
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? pilotLaps = await _persistence.OpenPractice.GetPilotOpenPracticeSessionLaps(disputed.SessionId, disputed.PilotId);

        Persistence.Abstractions.OpenPractice.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Persistence.Abstractions.OpenPractice.LapStatus.Completed
        && (x.Id != disputed.LapId || disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed));
        int lapCount = validLaps?.Count() ?? 0;

        Persistence.Abstractions.OpenPractice.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.FinishedUtc ?? lap?.StartedUtc;

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
        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? pilotLaps = await _persistence.OpenPractice.GetPilotOpenPracticeSessionLaps(removed.SessionId, removed.PilotId);

        Persistence.Abstractions.OpenPractice.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == removed.LapId);
        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Persistence.Abstractions.OpenPractice.LapStatus.Completed && x.Id != lap?.Id);
        int lapCount = validLaps?.Count() ?? 0;

        Persistence.Abstractions.OpenPractice.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.FinishedUtc ?? lap?.StartedUtc;

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
        IEnumerable<Persistence.Abstractions.OpenPractice.TotalLapLeaderboardPosition>? response = await _persistence.OpenPractice.GetOpenPracticeSessionTotalLapLeaderboardPositions(sessionId);

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