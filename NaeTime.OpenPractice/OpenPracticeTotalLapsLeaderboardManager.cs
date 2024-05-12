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
        var pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", completed.SessionId, completed.PilotId);

        var validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed && x.Id != completed.LapId);
        var lapCount = validLaps?.Count() ?? 0;

        lapCount++;

        var firstLap = validLaps?.FirstOrDefault();
        var firstLapCompletionUtc = firstLap?.FinishedUtc ?? completed.StartedUtc;

        await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new TotalLapRecord(lapCount, firstLapCompletionUtc));
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        var pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", disputed.SessionId, disputed.PilotId);

        var lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
        var validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed
        && (x.Id != disputed.LapId || disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed));
        var lapCount = validLaps?.Count() ?? 0;

        var firstLap = validLaps?.FirstOrDefault();
        var firstLapCompletionUtc = firstLap?.FinishedUtc ?? lap?.StartedUtc;

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
        var pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", removed.SessionId, removed.PilotId);

        var lap = pilotLaps?.FirstOrDefault(x => x.Id == removed.LapId);
        var validLaps = pilotLaps?.Where(x => x.Status == Messages.Models.LapStatus.Completed && x.Id != lap?.Id);
        var lapCount = validLaps?.Count() ?? 0;

        var firstLap = validLaps?.FirstOrDefault();
        var firstLapCompletionUtc = firstLap?.FinishedUtc ?? lap?.StartedUtc;

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
        var response = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.TotalLapLeaderboardPosition>>("GetOpenPracticeSessionTotalLapLeaderboardPositions", sessionId);

        return response?.Select(x => new LeaderboardPosition<TotalLapRecord>(x.PilotId, x.Position, new TotalLapRecord(x.TotalLaps, x.FirstLapCompletionUtc))) ?? Enumerable.Empty<LeaderboardPosition<TotalLapRecord>>();
    }
    protected override Task OnPositionImproved(Guid sessionId, Guid pilotId, int newPosition, int? oldPosition, TotalLapRecord newRecord)
        => _eventClient.Publish(new TotalLapsLeaderboardPositionImproved(sessionId, newPosition, oldPosition, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionReduced(Guid sessionId, Guid pilotId, int newPosition, int oldPosition, TotalLapRecord newRecord)
        => _eventClient.Publish(new TotalLapsLeaderboardPositionReduced(sessionId, newPosition, oldPosition, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionRemoved(Guid sessionId, Guid pilotId)
        => _eventClient.Publish(new TotalLapsLeaderboardPositionRemoved(sessionId, pilotId));
    protected override Task OnRecordImproved(Guid sessionId, Guid pilotId, TotalLapRecord newRecord)
        => _eventClient.Publish(new TotalLapsLeaderboardRecordImproved(sessionId, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnRecordReduced(Guid sessionId, Guid pilotId, TotalLapRecord newRecord)
        => _eventClient.Publish(new TotalLapsLeaderboardRecordReduced(sessionId, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
}