using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Models;
using NaeTime.PubSub.Abstractions;
namespace NaeTime.OpenPractice;
public class OpenPracticeConsecutiveLapsLeaderboardManager
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public OpenPracticeConsecutiveLapsLeaderboardManager(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(OpenPracticeLapCompleted completed)
    {
        IEnumerable<uint>? sessionTrackedLaps = await _rpcClient.InvokeAsync<IEnumerable<uint>>("GetOpenPracticeSessionTrackedConsecutiveLaps", completed.SessionId);
        if (sessionTrackedLaps == null || !sessionTrackedLaps.Any())
        {
            return;
        }

        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", completed.SessionId, completed.PilotId);
        IEnumerable<Messages.Models.ConsecutiveLapRecord>? pilotLapRecords = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.ConsecutiveLapRecord>>("", completed.SessionId, completed.PilotId);

        List<Lap> laps = new();

        if (pilotLaps?.Any() ?? false)
        {
            laps.AddRange(pilotLaps.Select(x => new Lap(x.Id, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   Messages.Models.LapStatus.Invalid => LapStatus.Invalid,
                                   Messages.Models.LapStatus.Completed => LapStatus.Completed,
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
        IEnumerable<uint>? sessionTrackedLaps = await _rpcClient.InvokeAsync<IEnumerable<uint>>("GetOpenPracticeSessionTrackedConsecutiveLaps", removed.SessionId);
        if (sessionTrackedLaps == null || !sessionTrackedLaps.Any())
        {
            return;
        }

        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", removed.SessionId, removed.PilotId);
        IEnumerable<Messages.Models.ConsecutiveLapRecord>? pilotLapRecords = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.ConsecutiveLapRecord>>("", removed.SessionId, removed.PilotId);

        List<Lap> laps = new();

        if (pilotLaps?.Any() ?? false)
        {
            laps.AddRange(pilotLaps.Select(x => new Lap(x.Id, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   Messages.Models.LapStatus.Invalid => LapStatus.Invalid,
                                   Messages.Models.LapStatus.Completed => LapStatus.Completed,
                                   _ => throw new NotImplementedException()
                               }, x.TotalMilliseconds)));
        }

        //Check that its not been removed already
        int removeIndex = laps.FindIndex(x => x.LapId == removed.LapId);
        if (removeIndex != -1)
        {
            laps.RemoveAt(removeIndex);
        }

        await FullTrackAndUpdateTrackedLaps(removed.SessionId, removed.PilotId, sessionTrackedLaps, pilotLapRecords, laps);
    }
    public async Task When(OpenPracticeLapDisputed disputed)
    {
        IEnumerable<uint>? sessionTrackedLaps = await _rpcClient.InvokeAsync<IEnumerable<uint>>("GetOpenPracticeSessionTrackedConsecutiveLaps", disputed.SessionId);
        if (sessionTrackedLaps == null || !sessionTrackedLaps.Any())
        {
            return;
        }

        IEnumerable<Messages.Models.Lap>? pilotLaps = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.Lap>>("GetPilotOpenPracticeSessionLaps", disputed.SessionId, disputed.PilotId);
        IEnumerable<Messages.Models.ConsecutiveLapRecord>? pilotLapRecords = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.ConsecutiveLapRecord>>("", disputed.SessionId, disputed.PilotId);

        List<Lap> laps = new();

        if (pilotLaps?.Any() ?? false)
        {
            laps.AddRange(pilotLaps.Select(x => new Lap(x.Id, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   Messages.Models.LapStatus.Invalid => LapStatus.Invalid,
                                   Messages.Models.LapStatus.Completed => LapStatus.Completed,
                                   _ => throw new NotImplementedException()
                               }, x.TotalMilliseconds)));
        }

        int existingLapIndex = laps.FindIndex(x => x.LapId == disputed.LapId);

        //We have no record of this lap so we can't do anything
        if (existingLapIndex < 0)
        {
            return;
        }

        LapStatus desiredStatus = disputed.ActualStatus switch
        {
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed => LapStatus.Completed,
            _ => throw new NotImplementedException()
        };

        Lap existingLap = laps[existingLapIndex];
        if (existingLap.Status != desiredStatus)
        {
            laps.RemoveAt(existingLapIndex);
            laps.Add(new Lap(existingLap.LapId, existingLap.StartedUtc, existingLap.FinishedUtc, desiredStatus, existingLap.TotalMilliseconds));
        }

        await FullTrackAndUpdateTrackedLaps(disputed.SessionId, disputed.PilotId, sessionTrackedLaps, pilotLapRecords, laps);
    }
    private async Task FullTrackAndUpdateTrackedLaps(Guid sessionId, Guid pilotId, IEnumerable<uint> sessionTrackedLaps, IEnumerable<Messages.Models.ConsecutiveLapRecord>? pilotLapRecords, List<Lap> laps)
    {
        laps.Sort((x, y) => x.FinishedUtc.CompareTo(y.FinishedUtc));
        FastestConsecutiveLapCalculator calculator = new();
        foreach (uint trackedLaps in sessionTrackedLaps)
        {
            Messages.Models.ConsecutiveLapRecord? pilotsExistingRecord = pilotLapRecords?.FirstOrDefault(x => x.LapCap == trackedLaps);

            ConsecutiveLapRecord? existingRecord = null;
            if (pilotsExistingRecord != null)
            {
                existingRecord = new ConsecutiveLapRecord(pilotsExistingRecord.TotalLaps, pilotsExistingRecord.TotalMilliseconds, pilotsExistingRecord.LastLapCompletionUtc, pilotsExistingRecord.IncludedLaps);
            }

            ConsecutiveLapRecord? newRecord = calculator.Calculate(trackedLaps, laps);

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
                int comparison = existingRecord.CompareTo(newRecord);

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
        IEnumerable<ConsecutiveLapsLeaderboardPosition>? existingLeaderboardPositions = await _rpcClient.InvokeAsync<IEnumerable<ConsecutiveLapsLeaderboardPosition>>("GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions", sessionId, lapCap);
        ConsecutiveLapsLeaderboard existingLeaderboard = new();
        ConsecutiveLapsLeaderboard newLeaderboard = new();

        if (existingLeaderboardPositions != null)
        {
            foreach (ConsecutiveLapsLeaderboardPosition existingLeaderboardPosition in existingLeaderboardPositions)
            {
                existingLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new ConsecutiveLapRecord(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));

                if (existingLeaderboardPosition.PilotId != pilotId)
                {
                    newLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new ConsecutiveLapRecord(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));
                }
            }
        }

        newLeaderboard.SetFastest(pilotId, new ConsecutiveLapRecord(totalLaps, totalMilliseconds, lastLapCompletionUtc, includedLaps));

        await CheckLeaderboards(sessionId, lapCap, pilotId, existingLeaderboard, newLeaderboard);
    }
    private async Task HandleRemovedRecord(Guid sessionId, Guid pilotId, uint lapCap)
    {
        IEnumerable<ConsecutiveLapsLeaderboardPosition>? existingLeaderboardPositions = await _rpcClient.InvokeAsync<IEnumerable<ConsecutiveLapsLeaderboardPosition>>("GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions", sessionId, lapCap);
        ConsecutiveLapsLeaderboard existingLeaderboard = new();
        ConsecutiveLapsLeaderboard newLeaderboard = new();

        if (existingLeaderboardPositions != null)
        {
            foreach (ConsecutiveLapsLeaderboardPosition existingLeaderboardPosition in existingLeaderboardPositions)
            {
                existingLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new ConsecutiveLapRecord(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));

                if (existingLeaderboardPosition.PilotId != pilotId)
                {
                    newLeaderboard.SetFastest(existingLeaderboardPosition.PilotId,
                    new ConsecutiveLapRecord(existingLeaderboardPosition.TotalLaps, existingLeaderboardPosition.TotalMilliseconds, existingLeaderboardPosition.LastLapCompletionUtc, existingLeaderboardPosition.IncludedLaps));
                }
            }
        }

        await CheckLeaderboards(sessionId, lapCap, pilotId, existingLeaderboard, newLeaderboard);
    }
    private async Task CheckLeaderboards(Guid sessionId, uint lapCap, Guid pilotId, ConsecutiveLapsLeaderboard existingLeaderboard, ConsecutiveLapsLeaderboard newLeaderboard)
    {
        IDictionary<Guid, ConsecutiveLapsLeaderboardPosition> existingPositions = existingLeaderboard.GetPositions();
        IDictionary<Guid, ConsecutiveLapsLeaderboardPosition> newPositions = newLeaderboard.GetPositions();

        foreach (KeyValuePair<Guid, ConsecutiveLapsLeaderboardPosition> existingPosition in existingPositions)
        {
            ConsecutiveLapsLeaderboardPosition existingRecord = existingPosition.Value;
            if (newPositions.TryGetValue(existingPosition.Key, out ConsecutiveLapsLeaderboardPosition? newRecord))
            {
                int positionMovement = existingRecord.Position.CompareTo(newRecord.Position);

                if (positionMovement > 0)
                {
                    await _eventClient.Publish(new ConsecutiveLapLeaderboardPositionImproved(sessionId, lapCap, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps));
                }
                else if (positionMovement < 0)
                {
                    await _eventClient.Publish(new ConsecutiveLapLeaderboardPositionReduced(sessionId, lapCap, newRecord.Position, existingRecord.Position, existingPosition.Key, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps));
                }
                //We only need to check the pilot we updates record as the rest should not have changed
                else if (existingRecord.PilotId == pilotId)
                {
                    int recordComparison = ComparePositions(existingRecord.TotalLaps, existingRecord.TotalMilliseconds, existingRecord.LastLapCompletionUtc, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc);
                    if (recordComparison > 0)
                    {
                        await _eventClient.Publish(new ConsecutiveLapLeaderboardRecordImproved(sessionId, lapCap, pilotId, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps));
                    }
                    else if (recordComparison < 0)
                    {
                        await _eventClient.Publish(new ConsecutiveLapLeaderboardRecordReduced(sessionId, lapCap, pilotId, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps));
                    }
                }
            }
            //We a removed position
            else
            {
                await _eventClient.Publish(new ConsecutiveLapLeaderboardPositionRemoved(sessionId, lapCap, existingRecord.PilotId));
            }
        }

        foreach (KeyValuePair<Guid, ConsecutiveLapsLeaderboardPosition> newPosition in newPositions)
        {
            ConsecutiveLapsLeaderboardPosition newRecord = newPosition.Value;
            if (!existingPositions.ContainsKey(newPosition.Key))
            {
                await _eventClient.Publish(new ConsecutiveLapLeaderboardPositionImproved(sessionId, lapCap, newRecord.Position, null, newPosition.Key, newRecord.TotalLaps, newRecord.TotalMilliseconds, newRecord.LastLapCompletionUtc, newRecord.IncludedLaps));
            }
        }
    }
}
