using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class OpenPracticeAverageLapLeaderboardManager : LeaderboardManager<AverageLapRecord>
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimePersistence _persistence;

    public OpenPracticeAverageLapLeaderboardManager(IEventClient eventClient, INaeTimePersistence persistence)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    public async Task When(OpenPracticeLapCompleted completed)
    {
        var pilotLaps = await _persistence.OpenPractice.GetPilotOpenPracticeSessionLaps(completed.SessionId, completed.PilotId);

        var validLaps = pilotLaps?.Where(x => x.Status == Persistence.Abstractions.OpenPractice.LapStatus.Completed && x.Id != completed.LapId).ToList() ?? new List<Persistence.Abstractions.OpenPractice.Lap>();

        if (!validLaps.Any(x => x.Id == completed.LapId))
        {
            validLaps.Add(new Persistence.Abstractions.OpenPractice.Lap(completed.LapId,
                completed.PilotId,
                completed.StartedUtc,
                completed.FinishedUtc,
                Persistence.Abstractions.OpenPractice.LapStatus.Completed,
                completed.TotalMilliseconds));
        }

        double average = validLaps?.Average(x => x.TotalMilliseconds) ?? 0;

        Persistence.Abstractions.OpenPractice.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime firstLapCompletionUtc = firstLap?.FinishedUtc ?? completed.StartedUtc;

        await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new AverageLapRecord(average, firstLapCompletionUtc));
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        var pilotLaps = await _persistence.OpenPractice.GetPilotOpenPracticeSessionLaps(disputed.SessionId, disputed.PilotId);

        Persistence.Abstractions.OpenPractice.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Persistence.Abstractions.OpenPractice.LapStatus.Completed
        && (x.Id != disputed.LapId || disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed));

        double average = validLaps == null || !validLaps.Any() ? 0 : validLaps.Average(x => x.TotalMilliseconds);

        Persistence.Abstractions.OpenPractice.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.FinishedUtc ?? lap?.StartedUtc;

        if (firstLapCompletionUtc == null)
        {
            await HandleUpdatedRecord(disputed.SessionId, disputed.PilotId, null);
        }
        else
        {
            await HandleUpdatedRecord(disputed.SessionId, disputed.PilotId, new AverageLapRecord(average, firstLapCompletionUtc.Value));
        }
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        var pilotLaps = await _persistence.OpenPractice.GetPilotOpenPracticeSessionLaps(removed.SessionId, removed.PilotId);

        Persistence.Abstractions.OpenPractice.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == removed.LapId);
        IEnumerable<Persistence.Abstractions.OpenPractice.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Persistence.Abstractions.OpenPractice.LapStatus.Completed && x.Id != lap?.Id);
        double average = 0;
        if (validLaps?.Any() ?? false)
        {
            average = validLaps.Average(x => x.TotalMilliseconds);
        }

        Persistence.Abstractions.OpenPractice.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.FinishedUtc ?? lap?.StartedUtc;

        if (firstLapCompletionUtc == null)
        {
            await HandleUpdatedRecord(removed.SessionId, removed.PilotId, null);
        }
        else
        {
            await HandleUpdatedRecord(removed.SessionId, removed.PilotId, new AverageLapRecord(average, firstLapCompletionUtc.Value));
        }
    }

    protected override async Task<IEnumerable<LeaderboardPosition<AverageLapRecord>>> GetExistingPositions(Guid sessionId)
    {
        IEnumerable<Persistence.Abstractions.OpenPractice.AverageLapLeaderboardPosition>? response = await _persistence.OpenPractice.GetOpenPracticeSessionAverageLapLeaderboardPositions(sessionId);

        return response?.Select(x => new LeaderboardPosition<AverageLapRecord>(x.PilotId, x.Position, new AverageLapRecord(x.AverageMilliseconds, x.FirstLapCompletion))) ?? Enumerable.Empty<LeaderboardPosition<AverageLapRecord>>();
    }
    protected override Task OnPositionImproved(Guid sessionId, Guid pilotId, int newPosition, int? oldPosition, AverageLapRecord newRecord)
    => _eventClient.PublishAsync(new AverageLapLeaderboardPositionImproved(sessionId, newPosition, oldPosition, pilotId, newRecord.AverageMilliseconds, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionReduced(Guid sessionId, Guid pilotId, int newPosition, int oldPosition, AverageLapRecord newRecord)
        => _eventClient.PublishAsync(new AverageLapLeaderboardPositionReduced(sessionId, newPosition, oldPosition, pilotId, newRecord.AverageMilliseconds, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionRemoved(Guid sessionId, Guid pilotId)
        => _eventClient.PublishAsync(new AverageLapLeaderboardPositionRemoved(sessionId, pilotId));
    protected override Task OnRecordImproved(Guid sessionId, Guid pilotId, AverageLapRecord newRecord)
        => _eventClient.PublishAsync(new AverageLapLeaderboardRecordImproved(sessionId, pilotId, newRecord.AverageMilliseconds, newRecord.FirstLapCompletionUtc));
    protected override Task OnRecordReduced(Guid sessionId, Guid pilotId, AverageLapRecord newRecord)
        => _eventClient.PublishAsync(new AverageLapLeaderboardRecordReduced(sessionId, pilotId, newRecord.AverageMilliseconds, newRecord.FirstLapCompletionUtc));
}
