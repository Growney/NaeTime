using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing;
using NaeTime.Timing.Models;

namespace NaeTime.OpenPractice;
public class OpenPracticeSingleLapLeaderboardManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public OpenPracticeSingleLapLeaderboardManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }
    public async Task When(OpenPracticeLapCompleted completed)
    {
        var existingRecord = await _publishSubscribe.Request<PilotSingleLapRecordRequest, PilotSingleLapRecordResponse>(new PilotSingleLapRecordRequest(completed.SessionId, completed.PilotId));

        if (existingRecord == null)
        {
            await HandleUpdatedRecord(completed.SessionId, completed.PilotId, completed.TotalMilliseconds, completed.FinishedUtc, completed.LapId);
        }
        else
        {
            if (ComparePositions(existingRecord.TotalMilliseconds, existingRecord.CompletionUtc, completed.TotalMilliseconds, completed.FinishedUtc) > 0)
            {
                await HandleUpdatedRecord(completed.SessionId, completed.PilotId, completed.TotalMilliseconds, completed.FinishedUtc, completed.LapId);
            }
        }
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        var existingRecord = await _publishSubscribe.Request<PilotSingleLapRecordRequest, PilotSingleLapRecordResponse>(new PilotSingleLapRecordRequest(disputed.SessionId, disputed.PilotId));

        //When its changed to completed its like we have a new lap se we need to check it against the existing record
        if (disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed)
        {
            var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(disputed.SessionId, disputed.PilotId));

            var lap = pilotLaps?.Laps.FirstOrDefault(x => x.Id == disputed.LapId);
            if (lap == null)
            {
                return;
            }
            //We have no existing record so it must be a new record
            if (existingRecord == null)
            {
                await HandleUpdatedRecord(disputed.SessionId, disputed.PilotId, lap.TotalMilliseconds, lap.FinishedUtc, lap.Id);
                return;
            }

            if (ComparePositions(existingRecord.TotalMilliseconds, existingRecord.CompletionUtc, lap.TotalMilliseconds, lap.FinishedUtc) > 0)
            {
                await HandleUpdatedRecord(disputed.SessionId, disputed.PilotId, lap.TotalMilliseconds, lap.FinishedUtc, lap.Id);
            }
        }
        else
        {
            //We have no existing record or its not the current record holding lap
            if (existingRecord != null && existingRecord.LapId != disputed.LapId)
            {
                return;
            }

            await HandleRemovedRecord(disputed.SessionId, disputed.PilotId, disputed.LapId);
        }
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        var existingRecord = await _publishSubscribe.Request<PilotSingleLapRecordRequest, PilotSingleLapRecordResponse>(new PilotSingleLapRecordRequest(removed.SessionId, removed.PilotId));

        if (existingRecord == null)
        {
            return;
        }

        if (existingRecord.LapId != removed.LapId)
        {
            return;
        }

        await HandleRemovedRecord(removed.SessionId, removed.PilotId, removed.LapId);
    }

    private async Task HandleRemovedRecord(Guid sessionId, Guid pilotId, Guid lapId)
    {
        var newFastest = await CalculatePilotsFastestSingle(sessionId, pilotId, lapId);

        //We removed but we have a new fastest so we can handle as if it was just a standard update
        if (newFastest != null)
        {
            await HandleUpdatedRecord(sessionId, pilotId, newFastest.LapMilliseconds, newFastest.CompletionUtc, newFastest.LapId);
            return;
        }


        //The pilot no longer has a new fastest so we need to redo the leaderboard without them in it
        var existingPositions = await _publishSubscribe.Request<SingleLapLeaderboardRequest, SingleLapLeaderboardResponse>(new SingleLapLeaderboardRequest(sessionId));

        var existingLeaderboard = new SingleLapLeaderboard();
        var newLeaderboard = new SingleLapLeaderboard();

        if (existingPositions != null)
        {
            foreach (var existingPosition in existingPositions.Positions)
            {
                existingLeaderboard.SetFastest(existingPosition.PilotId, existingPosition.LapId, existingPosition.TotalMilliseconds, existingPosition.CompletionUtc);

                if (existingPosition.PilotId != pilotId)
                {
                    newLeaderboard.SetFastest(existingPosition.PilotId, existingPosition.LapId, existingPosition.TotalMilliseconds, existingPosition.CompletionUtc);
                }
            }
        }

        await CheckLeaderboards(sessionId, pilotId, existingLeaderboard, newLeaderboard);
    }

    private IEnumerable<Lap> GetLaps(PilotLapsResponse response, Guid? excludedLapId)
    {
        foreach (var lap in response.Laps)
        {
            if (excludedLapId.HasValue && lap.Id == excludedLapId)
            {
                continue;
            }

            yield return new Lap(lap.Id, lap.StartedUtc, lap.FinishedUtc, lap.Status switch
            {
                PilotLapsResponse.LapStatus.Invalid => LapStatus.Invalid,
                PilotLapsResponse.LapStatus.Completed => LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, lap.TotalMilliseconds);
        }
    }
    private async Task<FastestSingleLap?> CalculatePilotsFastestSingle(Guid sessionId, Guid pilotId, Guid? excludedLapId)
    {
        var pilotLaps = await _publishSubscribe.Request<PilotLapsRequest, PilotLapsResponse>(new PilotLapsRequest(sessionId, pilotId));

        if (pilotLaps == null)
        {
            return null;
        }

        var calculator = new FastestSingleLapCalculator();

        return calculator.Calculate(GetLaps(pilotLaps, excludedLapId));
    }
    public async Task HandleUpdatedRecord(Guid sessionId, Guid pilotId, long totalMilliseconds, DateTime completionUtc, Guid lapId)
    {
        var existingPositions = await _publishSubscribe.Request<SingleLapLeaderboardRequest, SingleLapLeaderboardResponse>(new SingleLapLeaderboardRequest(sessionId));

        var existingLeaderboard = new SingleLapLeaderboard();
        var newLeaderboard = new SingleLapLeaderboard();

        if (existingPositions != null)
        {
            foreach (var existingPosition in existingPositions.Positions)
            {
                existingLeaderboard.SetFastest(existingPosition.PilotId, existingPosition.LapId, existingPosition.TotalMilliseconds, existingPosition.CompletionUtc);

                if (existingPosition.PilotId != pilotId)
                {
                    newLeaderboard.SetFastest(existingPosition.PilotId, existingPosition.LapId, existingPosition.TotalMilliseconds, existingPosition.CompletionUtc);
                }
            }
        }

        newLeaderboard.SetFastest(pilotId, lapId, totalMilliseconds, completionUtc);

        await CheckLeaderboards(sessionId, pilotId, existingLeaderboard, newLeaderboard);
    }

    public async Task CheckLeaderboards(Guid sessionId, Guid pilotId, SingleLapLeaderboard existingLeaderboard, SingleLapLeaderboard newLeaderboard)
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
                    await _publishSubscribe.Dispatch(new SingleLapLeaderboardPositionImproved(sessionId, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId));
                }
                else if (positionMovement < 0)
                {
                    await _publishSubscribe.Dispatch(new SingleLapLeaderboardPositionReduced(sessionId, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId));
                }
                //We only need to check the pilot we updates record as the rest should not have changed
                else if (existingRecord.PilotId == pilotId)
                {
                    var recordComparison = ComparePositions(existingRecord.LapMilliseconds, existingRecord.CompletionTime, newRecord.LapMilliseconds, newRecord.CompletionTime);
                    if (recordComparison > 0)
                    {
                        await _publishSubscribe.Dispatch(new SingleLapLeaderboardRecordImproved(sessionId, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId));
                    }
                    else if (recordComparison < 0)
                    {
                        await _publishSubscribe.Dispatch(new SingleLapLeaderboardRecordReduced(sessionId, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId));
                    }
                }
            }
            //We a removed position
            else
            {
                await _publishSubscribe.Dispatch(new SingleLapLeaderboardPositionRemoved(sessionId, existingRecord.PilotId));
            }
        }

        foreach (var newPosition in newPositions)
        {
            var newRecord = newPosition.Value;
            if (!existingPositions.ContainsKey(newPosition.Key))
            {
                await _publishSubscribe.Dispatch(new SingleLapLeaderboardPositionImproved(sessionId, newRecord.Position, null, newPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId));
            }
        }
    }

    private int ComparePositions(long xTotalMilliseconds, DateTime xLastLapCompletionUtc, long yTotalMilliseconds, DateTime yLastLapCompletionUtc)
    {
        int result = xTotalMilliseconds.CompareTo(yTotalMilliseconds);
        if (result != 0)
        {
            return result;
        }

        return xLastLapCompletionUtc.CompareTo(yLastLapCompletionUtc);
    }


}
