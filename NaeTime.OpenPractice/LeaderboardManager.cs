using NaeTime.OpenPractice.Leaderboards;

namespace NaeTime.OpenPractice;
public abstract class LeaderboardManager<TRecord> where TRecord : IComparable<TRecord>
{
    protected abstract Task OnPositionImproved(Guid sessionId, Guid pilotId, int newPosition, int? oldPosition, TRecord newRecord);
    protected abstract Task OnPositionReduced(Guid sessionId, Guid pilotId, int newPosition, int oldPosition, TRecord newRecord);
    protected abstract Task OnPositionRemoved(Guid sessionId, Guid pilotId);
    protected abstract Task OnRecordImproved(Guid sessionId, Guid pilotId, TRecord newRecord);
    protected abstract Task OnRecordReduced(Guid sessionId, Guid pilotId, TRecord newRecord);

    protected abstract Task<IEnumerable<LeaderboardPosition<TRecord>>> GetExistingPositions(Guid sessionId);

    protected async Task CheckLeaderboards(Guid sessionId, Guid pilotId, Leaderboard<TRecord> existingLeaderboard, Leaderboard<TRecord> newLeaderboard)
    {
        var existingPositions = existingLeaderboard.GetPositions();
        var newPositions = newLeaderboard.GetPositions();

        foreach (var existingPosition in existingPositions.Values)
        {
            if (newPositions.TryGetValue(existingPosition.PilotId, out var newPosition))
            {
                var positionMovement = existingPosition.Position.CompareTo(newPosition.Position);

                if (positionMovement > 0)
                {
                    await OnPositionImproved(sessionId, newPosition.PilotId, newPosition.Position, existingPosition.Position, newPosition.Record);
                }
                else if (positionMovement < 0)
                {
                    await OnPositionReduced(sessionId, newPosition.PilotId, newPosition.Position, existingPosition.Position, newPosition.Record);
                }
                //We only need to check the pilot we updates record as the rest should not have changed
                else if (existingPosition.PilotId == pilotId)
                {
                    var recordComparison = existingPosition.Record.CompareTo(newPosition.Record);
                    if (recordComparison > 0)
                    {
                        await OnRecordImproved(sessionId, pilotId, newPosition.Record);
                    }
                    else if (recordComparison < 0)
                    {
                        await OnRecordReduced(sessionId, pilotId, newPosition.Record);
                    }
                }
            }
            //We a removed position
            else
            {
                await OnPositionRemoved(sessionId, existingPosition.PilotId);
            }
        }

        foreach (var newPosition in newPositions)
        {
            var newRecord = newPosition.Value;
            if (!existingPositions.ContainsKey(newPosition.Key))
            {
                await OnPositionImproved(sessionId, newRecord.PilotId, newRecord.Position, null, newRecord.Record);
            }
        }
    }
    protected async Task HandleUpdatedRecord(Guid sessionId, Guid pilotId, TRecord? newRecord)
    {
        var existingPositions = await GetExistingPositions(sessionId);

        var existingLeaderboard = new Leaderboard<TRecord>();
        var newLeaderboard = new Leaderboard<TRecord>();

        if (existingPositions != null)
        {
            foreach (var existingPosition in existingPositions)
            {
                existingLeaderboard.SetFastest(existingPosition.PilotId, existingPosition.Record);

                if (existingPosition.PilotId != pilotId)
                {
                    newLeaderboard.SetFastest(existingPosition.PilotId, existingPosition.Record);
                }
            }
        }

        if (newRecord != null)
        {
            newLeaderboard.SetFastest(pilotId, newRecord);
        }

        await CheckLeaderboards(sessionId, pilotId, existingLeaderboard, newLeaderboard);
    }
}
