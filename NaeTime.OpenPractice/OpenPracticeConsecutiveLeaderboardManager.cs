using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
public class OpenPracticeConsecutiveLeaderboardManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public OpenPracticeConsecutiveLeaderboardManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }
    public async Task When(ConsecutiveLapRecordRemoved removed)
    {
        var existingLeaderboardPositions = await _publishSubscribe.Request<ConsecutiveLapLeaderboardRequest, ConsecutiveLapLeaderboardReponse>(new ConsecutiveLapLeaderboardRequest(removed.SessionId, removed.LapCap)).ConfigureAwait(false);
        var existingLeaderboardPosition = existingLeaderboardPositions?.Positions.FirstOrDefault(x => x.PilotId == removed.PilotId);

        if (existingLeaderboardPosition == null)
        {
            return;
        }

        var sortedExistingLeaderboardPositions = existingLeaderboardPositions?.Positions.ToList() ?? new List<ConsecutiveLapLeaderboardReponse.LeadboardPosition>();
        sortedExistingLeaderboardPositions.Sort((x, y) => x.Position.CompareTo(y.Position));

        await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionRemoved(removed.SessionId, removed.LapCap, removed.PilotId));
        for (int i = existingLeaderboardPosition.Position; i < sortedExistingLeaderboardPositions.Count(); i++)
        {
            var position = sortedExistingLeaderboardPositions[i];
            if (position.PilotId == removed.PilotId)
            {
                continue;
            }

            await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionImproved(removed.SessionId, removed.LapCap, position.Position + 1, position.Position, position.PilotId, position.TotalLaps, position.TotalMilliseconds, position.LastLapCompletionUtc, position.IncludedLaps));
        }
    }
    public async Task When(ConsecutiveLapRecordReduced reduced)
    {
        var existingLeaderboardPositions = await _publishSubscribe.Request<ConsecutiveLapLeaderboardRequest, ConsecutiveLapLeaderboardReponse>(new ConsecutiveLapLeaderboardRequest(reduced.SessionId, reduced.LapCap)).ConfigureAwait(false);
        var existingLeaderboardPosition = existingLeaderboardPositions?.Positions.FirstOrDefault(x => x.PilotId == reduced.PilotId);

        if (existingLeaderboardPosition == null)
        {
            return;
        }

        var sortedExistingLeaderboardPositions = existingLeaderboardPositions?.Positions.ToList() ?? new List<ConsecutiveLapLeaderboardReponse.LeadboardPosition>();
        sortedExistingLeaderboardPositions.Sort((x, y) => x.Position.CompareTo(y.Position));

        int insertIndex = 0;

        for (int i = existingLeaderboardPosition.Position; i < sortedExistingLeaderboardPositions.Count(); i++)
        {
            var position = sortedExistingLeaderboardPositions[i];
            if (position.PilotId == reduced.PilotId)
            {
                insertIndex = i;
                continue;
            }

            var comparison = ComparePositions(reduced.TotalLaps, reduced.TotalLaps, reduced.LastLapCompletionUtc, position.TotalLaps, position.TotalMilliseconds, position.LastLapCompletionUtc);

            if (comparison > 0)
            {
                insertIndex = i;
                break;
            }
        }

        if (insertIndex == existingLeaderboardPosition.Position)
        {
            await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardRecordReduced(reduced.SessionId, reduced.LapCap, reduced.PilotId, reduced.TotalLaps, reduced.TotalMilliseconds, reduced.LastLapCompletionUtc, reduced.IncludedLaps));
            return;
        }

        await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionImproved(reduced.SessionId, reduced.LapCap, insertIndex, existingLeaderboardPosition.Position, reduced.PilotId, reduced.TotalLaps, reduced.TotalMilliseconds, reduced.LastLapCompletionUtc, reduced.IncludedLaps));
        for (int i = existingLeaderboardPosition.Position; i < insertIndex; i++)
        {
            var position = sortedExistingLeaderboardPositions[i];
            if (position.PilotId == reduced.PilotId)
            {
                continue;
            }

            await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionImproved(reduced.SessionId, reduced.LapCap, position.Position + 1, position.Position, position.PilotId, position.TotalLaps, position.TotalMilliseconds, position.LastLapCompletionUtc, position.IncludedLaps));
        }

    }
    public async Task When(ConsecutiveLapRecordImproved improved)
    {
        var existingLeaderboardPositions = await _publishSubscribe.Request<ConsecutiveLapLeaderboardRequest, ConsecutiveLapLeaderboardReponse>(new ConsecutiveLapLeaderboardRequest(improved.SessionId, improved.LapCap)).ConfigureAwait(false);
        var existingLeaderboardPosition = existingLeaderboardPositions?.Positions.FirstOrDefault(x => x.PilotId == improved.PilotId);

        var sortedExistingLeaderboardPositions = existingLeaderboardPositions?.Positions.ToList() ?? new List<ConsecutiveLapLeaderboardReponse.LeadboardPosition>();
        sortedExistingLeaderboardPositions.Sort((x, y) => x.Position.CompareTo(y.Position));

        int insertIndex = 0;

        for (int i = 0; i < sortedExistingLeaderboardPositions.Count(); i++)
        {
            var position = sortedExistingLeaderboardPositions[i];
            if (position.PilotId == improved.PilotId)
            {
                insertIndex = i;
                continue;
            }

            var comparison = ComparePositions(improved.TotalLaps, improved.TotalLaps, improved.LastLapCompletionUtc, position.TotalLaps, position.TotalMilliseconds, position.LastLapCompletionUtc);

            if (comparison > 0)
            {
                insertIndex = i;
                break;
            }
        }

        if (existingLeaderboardPosition != null && insertIndex == existingLeaderboardPosition.Position)
        {
            await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardRecordImproved(improved.SessionId, improved.LapCap, improved.PilotId, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps));
            return;
        }

        await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionImproved(improved.SessionId, improved.LapCap, insertIndex, existingLeaderboardPosition?.Position, improved.PilotId, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps));

        for (int i = insertIndex; i < sortedExistingLeaderboardPositions.Count(); i++)
        {
            var position = sortedExistingLeaderboardPositions[i];

            await _publishSubscribe.Dispatch(new ConsecutiveLapLeaderboardPositionReduced(improved.SessionId, improved.LapCap, insertIndex + 1, insertIndex, improved.PilotId, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps));
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
}
