using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing;
using NaeTime.Timing.Models;
namespace NaeTime.OpenPractice;
public class OpenPracticeConsecutiveLapsLeaderboardManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public OpenPracticeConsecutiveLapsLeaderboardManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }

    public async Task When(OpenPracticeLapCompleted completed)
    {
        var sessionTrackedLaps = await _publishSubscribe.Request<TrackedConsecutiveLapsRequest, TrackedConsecutiveLapsResponse>(new TrackedConsecutiveLapsRequest(completed.SessionId));
        if (sessionTrackedLaps == null || !sessionTrackedLaps.ConsecutiveLaps.Any())
        {
            return;
        }

        var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(completed.SessionId, completed.PilotId)).ConfigureAwait(false);
        var pilotLapRecords = await _publishSubscribe.Request<PilotConsecutiveLapRecordsRequest, PilotConsecutiveLapRecordsResponse>(new PilotConsecutiveLapRecordsRequest(completed.SessionId, completed.PilotId));

        var laps = new List<Lap>();

        if (pilotLaps?.Laps.Any() ?? false)
        {
            laps.AddRange(pilotLaps.Laps.Select(x => new Lap(x.Id, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   PilotLapsResponse.LapStatus.Invalid => LapStatus.Invalid,
                                   PilotLapsResponse.LapStatus.Completed => LapStatus.Completed,
                                   _ => throw new NotImplementedException()
                               }, x.TotalMilliseconds)));
        }

        //Check that its not been added already
        if (!laps.Any(x => x.LapId == completed.LapId))
        {
            laps.Add(new Lap(completed.LapId, completed.StartedUtc, completed.FinishedUtc, LapStatus.Completed, completed.TotalMilliseconds));
        }

        laps.Sort((x, y) => x.FinishedUtc.CompareTo(y.FinishedUtc));

        await FullTrackAndUpdateTrackedLaps(completed.SessionId, completed.PilotId, sessionTrackedLaps, pilotLapRecords, laps);
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        var sessionTrackedLaps = await _publishSubscribe.Request<TrackedConsecutiveLapsRequest, TrackedConsecutiveLapsResponse>(new TrackedConsecutiveLapsRequest(removed.SessionId));
        if (sessionTrackedLaps == null || !sessionTrackedLaps.ConsecutiveLaps.Any())
        {
            return;
        }

        var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(removed.SessionId, removed.PilotId)).ConfigureAwait(false);
        var pilotLapRecords = await _publishSubscribe.Request<PilotConsecutiveLapRecordsRequest, PilotConsecutiveLapRecordsResponse>(new PilotConsecutiveLapRecordsRequest(removed.SessionId, removed.PilotId));

        var laps = new List<Lap>();

        if (pilotLaps?.Laps.Any() ?? false)
        {
            laps.AddRange(pilotLaps.Laps.Select(x => new Lap(x.Id, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   PilotLapsResponse.LapStatus.Invalid => LapStatus.Invalid,
                                   PilotLapsResponse.LapStatus.Completed => LapStatus.Completed,
                                   _ => throw new NotImplementedException()
                               }, x.TotalMilliseconds)));
        }

        //Check that its not been removed already
        var removeIndex = laps.FindIndex(x => x.LapId == removed.LapId);
        if (removeIndex != -1)
        {
            laps.RemoveAt(removeIndex);
        }

        await FullTrackAndUpdateTrackedLaps(removed.SessionId, removed.PilotId, sessionTrackedLaps, pilotLapRecords, laps);
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        var sessionTrackedLaps = await _publishSubscribe.Request<TrackedConsecutiveLapsRequest, TrackedConsecutiveLapsResponse>(new TrackedConsecutiveLapsRequest(disputed.SessionId));
        if (sessionTrackedLaps == null || !sessionTrackedLaps.ConsecutiveLaps.Any())
        {
            return;
        }

        var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(disputed.SessionId, disputed.PilotId)).ConfigureAwait(false);
        var pilotLapRecords = await _publishSubscribe.Request<PilotConsecutiveLapRecordsRequest, PilotConsecutiveLapRecordsResponse>(new PilotConsecutiveLapRecordsRequest(disputed.SessionId, disputed.PilotId));

        var laps = new List<Lap>();

        if (pilotLaps?.Laps.Any() ?? false)
        {
            laps.AddRange(pilotLaps.Laps.Select(x => new Lap(x.Id, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   PilotLapsResponse.LapStatus.Invalid => LapStatus.Invalid,
                                   PilotLapsResponse.LapStatus.Completed => LapStatus.Completed,
                                   _ => throw new NotImplementedException()
                               }, x.TotalMilliseconds)));
        }

        var existingLapIndex = laps.FindIndex(x => x.LapId == disputed.LapId);

        //We have no record of this lap so we can't do anything
        if (existingLapIndex < 0)
        {
            return;
        }

        var desiredStatus = disputed.ActualStatus switch
        {
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed => LapStatus.Completed,
            _ => throw new NotImplementedException()
        };

        var existingLap = laps[existingLapIndex];
        if (existingLap.Status != desiredStatus)
        {
            laps.RemoveAt(existingLapIndex);
            laps.Add(new Lap(existingLap.LapId, existingLap.StartedUtc, existingLap.FinishedUtc, desiredStatus, existingLap.TotalMilliseconds));
        }

        await FullTrackAndUpdateTrackedLaps(disputed.SessionId, disputed.PilotId, sessionTrackedLaps, pilotLapRecords, laps);
    }
    private async Task FullTrackAndUpdateTrackedLaps(Guid sessionId, Guid pilotId, TrackedConsecutiveLapsResponse sessionTrackedLaps, PilotConsecutiveLapRecordsResponse? pilotLapRecords, List<Lap> laps)
    {
        laps.Sort((x, y) => x.FinishedUtc.CompareTo(y.FinishedUtc));
        var calculator = new FastestConsecutiveLapCalculator();
        foreach (var trackedLaps in sessionTrackedLaps.ConsecutiveLaps)
        {
            var pilotsExistingRecord = pilotLapRecords?.Records.FirstOrDefault(x => x.LapCap == trackedLaps);

            FastestConsecutiveLaps? existingRecord = null;
            if (pilotsExistingRecord != null)
            {
                existingRecord = new FastestConsecutiveLaps(pilotsExistingRecord.TotalLaps, pilotsExistingRecord.TotalMilliseconds, pilotsExistingRecord.LastLapCompletionUtc, pilotsExistingRecord.IncludedLaps);
            }

            var newRecord = calculator.Calculate(trackedLaps, laps);

            //We have no new record and the existing record existed we should issues a remove event
            if (newRecord == null && existingRecord != null)
            {
                await HandleRemovedRecord(sessionId, pilotId, trackedLaps);
                continue;
            }
            // somehow we have removed a lap but we have no existing record and we have a new record so we should issue an improved, this could only happen if something happened when updating the existing record or something
            else if (newRecord != null && existingRecord == null)
            {
                await HandleUpdatedRecord(sessionId, pilotId, trackedLaps, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps);
            }
            else if (newRecord != null && existingRecord != null)
            {
                var comparison = existingRecord.CompareTo(newRecord);

                if (comparison > 0)
                {
                    await HandleUpdatedRecord(sessionId, pilotId, trackedLaps, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps);
                }
                else if (comparison < 0)
                {
                    await HandleUpdatedRecord(sessionId, pilotId, trackedLaps, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps);
                }
            }
        }
    }
    private int ComparePositions(uint xTotalLaps, long xTotalMilliseconds, DateTime xLastLapCompletionUtc, uint yTotalLaps, long yTotalMilliseconds, DateTime yLastLapCompletionUtc)
    {
        int result = yTotalLaps.CompareTo(xTotalLaps);
        if (result != 0)
        {
            return result;
        }

        result = xTotalMilliseconds.CompareTo(yTotalMilliseconds);
        if (result != 0)
        {
            return result;
        }

        return xLastLapCompletionUtc.CompareTo(yLastLapCompletionUtc);
    }

    private async Task HandleUpdatedRecord(Guid sessionId, Guid pilotId, uint lapCap, uint totalLaps, long totalMilliseconds, DateTime lastLapCompletionUtc, IEnumerable<Guid> includedLaps)
    {
        var existingLeaderboardPositions = await _publishSubscribe.Request<ConsecutiveLapLeaderboardRequest, ConsecutiveLapLeaderboardReponse>(new ConsecutiveLapLeaderboardRequest(sessionId, lapCap)).ConfigureAwait(false);
        var existingLeaderboard = new ConsecutiveLapsLeaderboard();
        var newLeaderboard = new ConsecutiveLapsLeaderboard();

        if (existingLeaderboardPositions != null)
        {
            foreach (var existingLeaderboardPosition in existingLeaderboardPositions.Positions)
            {
                existingLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new FastestConsecutiveLaps(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));

                if (existingLeaderboardPosition.PilotId != pilotId)
                {
                    newLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new FastestConsecutiveLaps(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));
                }
            }
        }

        newLeaderboard.SetFastest(pilotId, new FastestConsecutiveLaps(totalLaps, totalMilliseconds, lastLapCompletionUtc, includedLaps));

        await CheckLeaderboards(sessionId, lapCap, pilotId, existingLeaderboard, newLeaderboard);
    }
    private async Task HandleRemovedRecord(Guid sessionId, Guid pilotId, uint lapCap)
    {
        var existingLeaderboardPositions = await _publishSubscribe.Request<ConsecutiveLapLeaderboardRequest, ConsecutiveLapLeaderboardReponse>(new ConsecutiveLapLeaderboardRequest(sessionId, lapCap)).ConfigureAwait(false);
        var existingLeaderboard = new ConsecutiveLapsLeaderboard();
        var newLeaderboard = new ConsecutiveLapsLeaderboard();

        if (existingLeaderboardPositions != null)
        {
            foreach (var existingLeaderboardPosition in existingLeaderboardPositions.Positions)
            {
                existingLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new FastestConsecutiveLaps(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));

                if (existingLeaderboardPosition.PilotId != pilotId)
                {
                    newLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new FastestConsecutiveLaps(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));
                }
            }
        }

        await CheckLeaderboards(sessionId, lapCap, pilotId, existingLeaderboard, newLeaderboard);
    }
    private async Task CheckLeaderboards(Guid sessionId, uint lapCap, Guid pilotId, ConsecutiveLapsLeaderboard existingLeaderboard, ConsecutiveLapsLeaderboard newLeaderboard)
    {
        var existingPositions = existingLeaderboard.GetPositions();
        var newPositions = newLeaderboard.GetPositions();

        foreach (var existingPosition in existingPositions)
        {
            var existingRecord = existingPosition.Value;
            if (newPositions.TryGetValue(existingPosition.Key, out var newRecord))
            {
                var positionMovement = existingRecord.Position.CompareTo(newRecord.Position);

                if (positionMovement > 0)
                {
                    await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionImproved(sessionId, lapCap, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletion, newRecord.IncludedLaps));
                }
                else if (positionMovement < 0)
                {
                    await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionReduced(sessionId, lapCap, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletion, newRecord.IncludedLaps));
                }
                //We only need to check the pilot we updates record as the rest should not have changed
                else if (existingRecord.PilotId == pilotId)
                {
                    var recordComparison = ComparePositions(existingRecord.TotalLaps, existingRecord.TotalMilliseconds, existingRecord.LastLapCompletion, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletion);
                    if (recordComparison > 0)
                    {
                        await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardRecordImproved(sessionId, lapCap, pilotId, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletion, newRecord.IncludedLaps));
                    }
                    else if (recordComparison < 0)
                    {
                        await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardRecordReduced(sessionId, lapCap, pilotId, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletion, newRecord.IncludedLaps));
                    }
                }
            }
            //We a removed position
            else
            {
                await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionRemoved(sessionId, lapCap, existingRecord.PilotId));
            }
        }

        foreach (var newPosition in newPositions)
        {
            var newRecord = newPosition.Value;
            if (!existingPositions.ContainsKey(newPosition.Key))
            {
                await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionImproved(sessionId, lapCap, newRecord.Position, null, newPosition.Key, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletion, newRecord.IncludedLaps));
            }
        }
    }
}
