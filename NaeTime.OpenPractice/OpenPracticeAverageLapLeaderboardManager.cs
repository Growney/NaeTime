using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class OpenPracticeAverageLapLeaderboardManager : LeaderboardManager<AverageLapRecord>
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public OpenPracticeAverageLapLeaderboardManager(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(OpenPracticeLapCompleted completed)
    {
        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", completed.SessionId, completed.PilotId);

        List<Messages.Models.Lap> validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed && x.Id != completed.LapId).ToList() ?? new List<Messages.Models.Lap>();

        if (!validLaps.Any(x => x.Id == completed.LapId))
        {
            validLaps.Add(new Messages.Models.Lap(completed.LapId,
                completed.PilotId,
                completed.StartedUtc,
                completed.FinishedUtc,
                Messages.Models.LapStatus.Completed,
                completed.TotalMilliseconds));
        }

        double average = validLaps?.Average(x => x.TotalMilliseconds) ?? 0;

        Messages.Models.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime firstLapCompletionUtc = firstLap?.FinishedUtc ?? completed.StartedUtc;

        await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new AverageLapRecord(average, firstLapCompletionUtc));
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", disputed.SessionId, disputed.PilotId);

        Messages.Models.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
        IEnumerable<Messages.Models.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed
        && (x.Id != disputed.LapId || disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed));

        double average = validLaps?.Average(x => x.TotalMilliseconds) ?? 0;

        Messages.Models.Lap? firstLap = validLaps?.FirstOrDefault();
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
        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", removed.SessionId, removed.PilotId);

        Messages.Models.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == removed.LapId);
        IEnumerable<Messages.Models.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed && x.Id != lap?.Id);
        double average = 0;
        if (validLaps?.Any() ?? false)
        {
            average = validLaps.Average(x => x.TotalMilliseconds);
        }
        Messages.Models.Lap? firstLap = validLaps?.FirstOrDefault();
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
        IEnumerable<Messages.Models.AverageLapLeaderboardPosition>? response = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.AverageLapLeaderboardPosition>>("GetOpenPracticeSessionAverageLapLeaderboardPositions", sessionId);

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
