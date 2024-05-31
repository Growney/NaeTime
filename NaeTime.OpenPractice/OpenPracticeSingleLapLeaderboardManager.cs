using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Models;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing;

namespace NaeTime.OpenPractice;
public class OpenPracticeSingleLapLeaderboardManager
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public OpenPracticeSingleLapLeaderboardManager(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(OpenPracticeLapCompleted completed)
    {
        Messages.Models.SingleLapRecord? existingRecord = await _rpcClient.InvokeAsync<Messages.Models.SingleLapRecord?>("GetPilotOpenPracticeSessionSingleLapRecord", completed.SessionId, completed.PilotId);

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
        Messages.Models.SingleLapRecord? existingRecord = await _rpcClient.InvokeAsync<Messages.Models.SingleLapRecord?>("GetPilotOpenPracticeSessionSingleLapRecord", disputed.SessionId, disputed.PilotId);

        //When its changed to completed its like we have a new lap se we need to check it against the existing record
        if (disputed.ActualStatus == OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed)
        {
            IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", disputed.SessionId, disputed.PilotId);

            Messages.Models.Lap? lap = pilotLaps?.FirstOrDefault(x => x.Id == disputed.LapId);
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
        Messages.Models.SingleLapRecord? existingRecord = await _rpcClient.InvokeAsync<Messages.Models.SingleLapRecord?>("GetPilotOpenPracticeSessionSingleLapRecord", removed.SessionId, removed.PilotId);

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
        SingleLapRecord? newFastest = await CalculatePilotsFastestSingle(sessionId, pilotId, lapId);

        //We removed but we have a new fastest so we can handle as if it was just a standard update
        if (newFastest != null)
        {
            await HandleUpdatedRecord(sessionId, pilotId, newFastest.LapMilliseconds, newFastest.CompletionUtc, newFastest.LapId);
            return;
        }


        //The pilot no longer has a new fastest so we need to redo the leaderboard without them in it
        IEnumerable<Messages.Models.SingleLapLeaderboardPosition>? existingPositions = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.SingleLapLeaderboardPosition>>("GetOpenPracticeSessionSingleLapLeaderboardPositions", sessionId);

        SingleLapLeaderboard existingLeaderboard = new();
        SingleLapLeaderboard newLeaderboard = new();

        if (existingPositions != null)
        {
            foreach (Messages.Models.SingleLapLeaderboardPosition existingPosition in existingPositions)
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

    private IEnumerable<Lap> GetLaps(IEnumerable<Messages.Models.Lap> response, Guid? excludedLapId)
    {
        foreach (Messages.Models.Lap lap in response)
        {
            if (excludedLapId.HasValue && lap.Id == excludedLapId)
            {
                continue;
            }

            yield return new Lap(lap.Id, lap.StartedUtc, lap.FinishedUtc, lap.Status switch
            {
                Messages.Models.LapStatus.Invalid => LapStatus.Invalid,
                Messages.Models.LapStatus.Completed => LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, lap.TotalMilliseconds);
        }
    }
    private async Task<SingleLapRecord?> CalculatePilotsFastestSingle(Guid sessionId, Guid pilotId, Guid? excludedLapId)
    {
        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", sessionId, pilotId);

        if (pilotLaps == null)
        {
            return null;
        }

        FastestSingleLapCalculator calculator = new();

        return calculator.Calculate(GetLaps(pilotLaps, excludedLapId));
    }
    public async Task HandleUpdatedRecord(Guid sessionId, Guid pilotId, long totalMilliseconds, DateTime completionUtc, Guid lapId)
    {
        IEnumerable<Messages.Models.SingleLapLeaderboardPosition>? existingPositions = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.SingleLapLeaderboardPosition>>("GetOpenPracticeSessionSingleLapLeaderboardPositions", sessionId);

        SingleLapLeaderboard existingLeaderboard = new();
        SingleLapLeaderboard newLeaderboard = new();

        if (existingPositions != null)
        {
            foreach (Messages.Models.SingleLapLeaderboardPosition existingPosition in existingPositions)
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
        IDictionary<Guid, SingleLapLeaderboardPosition> existingPositions = existingLeaderboard.GetPositions();
        IDictionary<Guid, SingleLapLeaderboardPosition> newPositions = newLeaderboard.GetPositions();

        foreach (KeyValuePair<Guid, SingleLapLeaderboardPosition> existingPosition in existingPositions)
        {
            SingleLapLeaderboardPosition existingRecord = existingPosition.Value;
            if (newPositions.TryGetValue(existingPosition.Key, out SingleLapLeaderboardPosition? newRecord))
            {
                int positionMovement = existingRecord.Position.CompareTo(newRecord.Position);

                if (positionMovement > 0)
                {
                    await _eventClient.PublishAsync(new SingleLapLeaderboardPositionImproved(sessionId, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId, true));
                }
                else if (positionMovement < 0)
                {
                    await _eventClient.PublishAsync(new SingleLapLeaderboardPositionReduced(sessionId, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId, true));
                }
                //We only need to check the pilot we updates record as the rest should not have changed
                else if (existingRecord.PilotId == pilotId)
                {
                    int recordComparison = ComparePositions(existingRecord.LapMilliseconds, existingRecord.CompletionTime, newRecord.LapMilliseconds, newRecord.CompletionTime);
                    if (recordComparison > 0)
                    {
                        await _eventClient.PublishAsync(new SingleLapLeaderboardRecordImproved(sessionId, newRecord.Position, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId, true));
                    }
                    else if (recordComparison < 0)
                    {
                        await _eventClient.PublishAsync(new SingleLapLeaderboardRecordReduced(sessionId, newRecord.Position, existingPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId, true));
                    }
                }
            }
            //We a removed position
            else
            {
                await _eventClient.PublishAsync(new SingleLapLeaderboardPositionRemoved(sessionId, existingRecord.PilotId));
            }
        }

        foreach (KeyValuePair<Guid, SingleLapLeaderboardPosition> newPosition in newPositions)
        {
            SingleLapLeaderboardPosition newRecord = newPosition.Value;
            if (!existingPositions.ContainsKey(newPosition.Key))
            {
                await _eventClient.PublishAsync(new SingleLapLeaderboardPositionImproved(sessionId, newRecord.Position, null, newPosition.Key, newRecord.LapMilliseconds, newRecord.CompletionTime, newRecord.LapId, false));
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
