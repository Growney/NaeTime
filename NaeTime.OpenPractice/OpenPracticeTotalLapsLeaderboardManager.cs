using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class OpenPracticeTotalLapsLeaderboardManager : LeaderboardManager<TotalLapRecord>, ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public OpenPracticeTotalLapsLeaderboardManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }
    public async Task When(OpenPracticeLapCompleted completed)
    {
        var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(completed.SessionId, completed.PilotId));

        var validLaps = pilotLaps?.Laps.Where(x => x.Status == PilotLapsResponse.LapStatus.Completed && x.Id != completed.LapId);
        var lapCount = validLaps?.Count() ?? 0;

        lapCount++;

        var firstLap = validLaps?.FirstOrDefault();
        var firstLapCompletionUtc = firstLap?.FinishedUtc ?? completed.StartedUtc;

        await HandleUpdatedRecord(completed.SessionId, completed.PilotId, new TotalLapRecord(lapCount, firstLapCompletionUtc));
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(disputed.SessionId, disputed.PilotId));

        var lap = pilotLaps?.Laps.FirstOrDefault(x => x.Id == disputed.LapId);
        var validLaps = pilotLaps?.Laps.Where(x => x.Status == PilotLapsResponse.LapStatus.Completed && x.Id != lap?.Id);
        var lapCount = validLaps?.Count() ?? 0;

        if (disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed)
        {
            lapCount++;
        }
        else
        {
            lapCount--;
        }

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
        var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(removed.SessionId, removed.PilotId));

        var lap = pilotLaps?.Laps.FirstOrDefault(x => x.Id == removed.LapId);
        var validLaps = pilotLaps?.Laps.Where(x => x.Status == PilotLapsResponse.LapStatus.Completed && x.Id != lap?.Id);
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
        var response = await _publishSubscribe.Request<TotalLapsLeaderboardRequest, TotalLapsLeaderboardResponse>(new TotalLapsLeaderboardRequest(sessionId));

        return response?.Positions.Select(x => new LeaderboardPosition<TotalLapRecord>(x.PilotId, x.Position, new TotalLapRecord(x.TotalLaps, x.FirstLapCompletionUtc))) ?? Enumerable.Empty<LeaderboardPosition<TotalLapRecord>>();
    }
    protected override Task OnPositionImproved(Guid sessionId, Guid pilotId, int newPosition, int? oldPosition, TotalLapRecord newRecord)
        => _publishSubscribe.Dispatch(new TotalLapsLeaderboardPositionImproved(sessionId, newPosition, oldPosition, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionReduced(Guid sessionId, Guid pilotId, int newPosition, int oldPosition, TotalLapRecord newRecord)
        => _publishSubscribe.Dispatch(new TotalLapsLeaderboardPositionReduced(sessionId, newPosition, oldPosition, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnPositionRemoved(Guid sessionId, Guid pilotId)
        => _publishSubscribe.Dispatch(new TotalLapsLeaderboardPositionRemoved(sessionId, pilotId));
    protected override Task OnRecordImproved(Guid sessionId, Guid pilotId, TotalLapRecord newRecord)
        => _publishSubscribe.Dispatch(new TotalLapsLeaderboardRecordImproved(sessionId, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
    protected override Task OnRecordReduced(Guid sessionId, Guid pilotId, TotalLapRecord newRecord)
        => _publishSubscribe.Dispatch(new TotalLapsLeaderboardRecordReduced(sessionId, pilotId, newRecord.TotalLaps, newRecord.FirstLapCompletionUtc));
}