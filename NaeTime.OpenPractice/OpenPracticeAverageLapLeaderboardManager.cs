using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Persistence.Abstractions.Timing;
using NaeTime.Persistence.Abstractions.Timing.Extensions;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class OpenPracticeAverageLapLeaderboardManager : LeaderboardManager<AverageLapRecord>
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimeOrchestrator _orchestrator;

    public OpenPracticeAverageLapLeaderboardManager(IEventClient eventClient, INaeTimeOrchestrator orchestrator)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    //public async Task When(OpenPracticeLapCompleted completed)
    //{
    //    var pilotLaps = await _orchestrator.OpenPractice.GetPilotOpenPracticeSessionLaps(completed.SessionId, completed.PilotId);

    //    var validLaps = pilotLaps?.Where(x => x.Status == Persistence.Abstractions.OpenPractice.LapStatus.Vaild && x.Id != completed.LapId).ToList() ?? new List<Persistence.Abstractions.OpenPractice.OpenPracticeLap>();

    //    if (!validLaps.Any(x => x.Id == completed.LapId))
    //    {
    //        validLaps.Add(new Persistence.Abstractions.OpenPractice.Lap(completed.LapId,
    //            completed.PilotId,
    //            completed.StartedUtc,
    //            completed.FinishedUtc,
    //            Persistence.Abstractions.OpenPractice.LapStatus.Vaild,
    //            completed.TotalMilliseconds));
    //    }

    //    double average = validLaps.Average(x => Persistence.Abstractions.IDetectionExtensions.MillisecondsBetween(x.EntryDetection, x.ExitDetection ?? throw new InvalidOperationException("")));

    //    Persistence.Abstractions.OpenPractice.OpenPracticeLap? firstLap = validLaps?.FirstOrDefault();
    //    DateTime firstLapCompletionUtc = firstLap?.ExitDetection?.UtcTime ?? completed.StartedUtc;

    //    await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new AverageLapRecord(average, firstLapCompletionUtc));
    //}
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        var pilotLaps = await _orchestrator.Timing.GetPilotOpenPracticeSessionLaps(disputed.SessionId, disputed.PilotId);

        Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
        IEnumerable<Lap>? validLaps = pilotLaps?.Where(x => x.Status == LapStatus.Valid
        && (x.Id != disputed.LapId || disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Valid));

        double average = validLaps == null || !validLaps.Any() ? 0 : validLaps.Average(x => IDetectionExtensions.MillisecondsBetween(x.EntryDetection, x.ExitDetection ?? throw new InvalidOperationException("")));
        ;

        Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.ExitDetection?.UtcTime ?? lap?.EntryDetection.UtcTime;

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
        var pilotLaps = await _orchestrator.Timing.GetPilotOpenPracticeSessionLaps(removed.SessionId, removed.PilotId);

        Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == removed.LapId);
        IEnumerable<Lap>? validLaps = pilotLaps?.Where(x => x.Status == LapStatus.Valid && x.Id != lap?.Id);
        double average = 0;
        if (validLaps?.Any() ?? false)
        {
            average = validLaps.Average(x => IDetectionExtensions.MillisecondsBetween(x.EntryDetection, x.ExitDetection ?? throw new InvalidOperationException("")));
        }

        Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime? firstLapCompletionUtc = firstLap?.ExitDetection?.UtcTime ?? lap?.EntryDetection.UtcTime;

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
        IEnumerable<Persistence.Abstractions.OpenPractice.AverageLapLeaderboardPosition>? response = await _orchestrator.OpenPractice.GetOpenPracticeSessionAverageLapLeaderboardPositions(sessionId);

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
