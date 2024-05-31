using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class OpenPracticeTotalLapsLeaderboardManager : LeaderboardManager<TotalLapRecord>
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public OpenPracticeTotalLapsLeaderboardManager(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }
    public async Task When(OpenPracticeLapCompleted completed)
    {
        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", completed.SessionId, completed.PilotId);

        IEnumerable<Messages.Models.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed && x.Id != completed.LapId);
        int lapCount = validLaps?.Count() ?? 0;

        lapCount++;

        Messages.Models.Lap? firstLap = validLaps?.FirstOrDefault();
        DateTime firstLapCompletionUtc = firstLap?.FinishedUtc ?? completed.StartedUtc;

        await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new TotalLapRecord(lapCount, firstLapCompletionUtc));
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", disputed.SessionId, disputed.PilotId);

        Messages.Models.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
        IEnumerable<Messages.Models.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed
        && (x.Id != disputed.LapId || disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed));
        int lapCount = validLaps?.Count() ?? 0;

        Messages.Models.Lap? firstLap = validLaps?.FirstOrDefault();
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
        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", removed.SessionId, removed.PilotId);

        Messages.Models.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == removed.LapId);
        IEnumerable<Messages.Models.Lap>? validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed && x.Id != lap?.Id);
        int lapCount = validLaps?.Count() ?? 0;

        Messages.Models.Lap? firstLap = validLaps?.FirstOrDefault();
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
        IEnumerable<Messages.Models.TotalLapLeaderboardPosition>? response = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.TotalLapLeaderboardPosition>>("GetOpenPracticeSessionTotalLapLeaderboardPositions", sessionId);

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