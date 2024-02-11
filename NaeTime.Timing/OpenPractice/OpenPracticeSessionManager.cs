using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Timing.Practice;
public class OpenPracticeSessionManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public OpenPracticeSessionManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }

    public async Task When(LapCompleted lapCompleted)
    {
        var sessionResponse = await _publishSubscribe.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(lapCompleted.SessionId));

        if (sessionResponse == null)
        {
            return;
        }

        var pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapCompleted.Lane);
        if (pilotLane == null)
        {
            return;
        }

        await _publishSubscribe.Dispatch(new OpenPracticeLapCompleted(Guid.NewGuid(), lapCompleted.SessionId, pilotLane.PilotId, lapCompleted.LapNumber, lapCompleted.StartedUtcTime, lapCompleted.FinishedUtcTime, lapCompleted.TotalTime));
        OpenPracticeSession session = BuildSession(lapCompleted, sessionResponse);

        await CheckLeaderboards(lapCompleted, sessionResponse, pilotLane, session);
    }

    private static OpenPracticeSession BuildSession(LapCompleted lapCompleted, OpenPracticeSessionResponse sessionResponse)
    {
        var session = new OpenPracticeSession(lapCompleted.SessionId);

        foreach (var lap in sessionResponse.Laps)
        {
            switch (lap.Status)
            {
                case OpenPracticeSessionResponse.LapStatus.Invalid:
                    session.AddInvalidLap(lap.PilotId, lap.LapNumber, lap.StartedUtc, lap.FinishedUtc, lap.TotalMilliseconds);
                    break;
                case OpenPracticeSessionResponse.LapStatus.Completed:
                    session.AddCompletedLap(lap.PilotId, lap.LapNumber, lap.StartedUtc, lap.FinishedUtc, lap.TotalMilliseconds);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return session;
    }

    private async Task CheckLeaderboards(LapCompleted lapCompleted, OpenPracticeSessionResponse sessionResponse, OpenPracticeSessionResponse.PilotLane pilotLane, OpenPracticeSession session)
    {
        foreach (var leaderboard in sessionResponse.Leaderboards)
        {
            switch (leaderboard.Type)
            {
                case OpenPracticeSessionResponse.LeaderboardType.SingleLap:
                    await CheckSingleLapLeaderboard(leaderboard.LeaderboardId, lapCompleted, pilotLane.PilotId, session);
                    break;
                case OpenPracticeSessionResponse.LeaderboardType.ConsecutiveLaps:
                    if (leaderboard.ConsecutiveLaps.HasValue)
                    {
                        await CheckConsecutiveLeaderboard(leaderboard.LeaderboardId, leaderboard.ConsecutiveLaps.Value, lapCompleted, pilotLane.PilotId, session);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private async Task CheckConsecutiveLeaderboard(Guid leaderboardId, uint lapCap, LapCompleted newLap, Guid pilotId, OpenPracticeSession session)
    {
        var existingLeaderboard = session.GetConsecutiveLapLeaderboardPositions(lapCap);
        var newLeaderboard = session.GetConsecutiveLapLeaderboardPositionsWithNewCompletedLap(lapCap, pilotId, newLap.LapNumber, newLap.StartedUtcTime, newLap.FinishedUtcTime, newLap.TotalTime);

        if (!existingLeaderboard.SequenceEqual(newLeaderboard))
        {
            await _publishSubscribe.Dispatch(new OpenPracticeConsecutiveLapLeaderboardPositionsChanged(session.SessionId, leaderboardId,
                existingLeaderboard.Select(x => new OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds)),
                newLeaderboard.Select(x => new OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds))));
        }
    }
    private async Task CheckSingleLapLeaderboard(Guid leaderboardId, LapCompleted newLap, Guid pilotId, OpenPracticeSession session)
    {
        var existingLeaderboard = session.GetSingleLapLeaderboardPositions();
        var newLeaderboard = session.GetSingleLapLeaderboardPositionsWithNewCompletedLap(pilotId, newLap.LapNumber, newLap.StartedUtcTime, newLap.FinishedUtcTime, newLap.TotalTime);

        if (!existingLeaderboard.SequenceEqual(newLeaderboard))
        {
            await _publishSubscribe.Dispatch(new OpenPracticeSingleLapLeaderboardPositionsChanged(session.SessionId, leaderboardId,
                existingLeaderboard.Select(x => new OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds)),
                newLeaderboard.Select(x => new OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds))));
        }
    }
}
