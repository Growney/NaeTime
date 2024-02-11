using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Models;
using NaeTime.Timing.OpenPractice.Leaderboards;

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

        var singleLapLeaderboards = BuildSingleLapLeaderboards(sessionResponse.SingleLapLeaderboards);
        var consecutiveLapLeaderboards = BuildConsecutiveLapLeaderboard(sessionResponse.ConsecutiveLapLeaderboards);

        if (singleLapLeaderboards.Any() || consecutiveLapLeaderboards.Any())
        {
            var pilotLaps = sessionResponse.Laps.Where(x => x.PilotId == pilotLane.PilotId).Select(x => new Lap(x.LapNumber, x.StartedUtc, x.FinishedUtc,
                x.Status switch
                {
                    OpenPracticeSessionResponse.LapStatus.Invalid => LapStatus.Invalid,
                    OpenPracticeSessionResponse.LapStatus.Completed => LapStatus.Completed,
                    _ => throw new NotImplementedException()
                }, x.TotalMilliseconds)).ToList();

            pilotLaps.Add(new Lap(lapCompleted.LapNumber, lapCompleted.StartedUtcTime, lapCompleted.FinishedUtcTime, LapStatus.Completed, lapCompleted.TotalTime));

            foreach (var singlelapLeaderboard in singleLapLeaderboards)
            {
                await CheckSingleLapLeaderboard(sessionResponse.SessionId, pilotLane.PilotId, singlelapLeaderboard, pilotLaps);
            }
            foreach (var consecutiveLapLeaderboard in consecutiveLapLeaderboards)
            {
                await CheckConsecutiveLapLeaderboards(sessionResponse.SessionId, pilotLane.PilotId, consecutiveLapLeaderboard, pilotLaps);
            }
        }
    }


    private IEnumerable<SingleLapLeaderboard> BuildSingleLapLeaderboards(IEnumerable<OpenPracticeSessionResponse.SingleLapLeaderboard> sessionResponse)
    {
        var singleLapLeaderboards = new List<SingleLapLeaderboard>();

        foreach (var singleLapLeaderboard in sessionResponse)
        {
            var leaderboard = new SingleLapLeaderboard(singleLapLeaderboard.LeaderboardId);
            foreach (var position in singleLapLeaderboard.Positions)
            {
                leaderboard.AddFastestSingleLap(position.PilotId, position.LapNumber, position.TotalMilliseconds, position.CompletionUtc);
            }
            singleLapLeaderboards.Add(leaderboard);
        }

        return singleLapLeaderboards;
    }
    private IEnumerable<ConsecutiveLapLeaderboard> BuildConsecutiveLapLeaderboard(IEnumerable<OpenPracticeSessionResponse.ConsecutiveLapLeaderboard> sessionResponse)
    {
        var consecutiveLapLeaderboards = new List<ConsecutiveLapLeaderboard>();

        foreach (var consecutiveLapLeaderboard in sessionResponse)
        {
            var leaderboard = new ConsecutiveLapLeaderboard(consecutiveLapLeaderboard.LeaderboardId, consecutiveLapLeaderboard.ConsecutiveLaps);
            foreach (var position in consecutiveLapLeaderboard.Positions)
            {
                leaderboard.AddFastestConsecutiveLap(position.PilotId, position.StartLapNumber, position.EndLapNumber, position.TotalLaps, position.TotalMilliseconds, position.LastLapCompletionUtc);
            }
            consecutiveLapLeaderboards.Add(leaderboard);
        }

        return consecutiveLapLeaderboards;
    }

    private async Task CheckSingleLapLeaderboard(Guid sessionId, Guid pilotId, SingleLapLeaderboard leaderboard, IEnumerable<Lap> laps)
    {
        var calculator = new FastestSingleLapCalculator();
        var fastestSingle = calculator.Calculate(laps);

        if (fastestSingle == null)
        {
            return;
        }
        var oldPositions = leaderboard.GetPositions();

        if (leaderboard.AddFastestSingleLap(pilotId, fastestSingle.LapNumber, fastestSingle.LapMilliseconds, fastestSingle.CompletionUtc)) ;
        {
            var newPositions = leaderboard.GetPositions();
            await _publishSubscribe.Dispatch(new OpenPracticeSingleLapLeaderboardPositionsChanged(sessionId, leaderboard.LeaderboardId,
                oldPositions.Select(x => new OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds)),
                newPositions.Select(x => new OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds))));
        }

    }
    private async Task CheckConsecutiveLapLeaderboards(Guid sessionId, Guid pilotId, ConsecutiveLapLeaderboard leaderboard, IEnumerable<Lap> laps)
    {
        var calculator = new FastestConsecutiveLapCalculator();
        var consecutive = calculator.CalculateFastestConsecutiveLaps(leaderboard.LapCount, laps);

        if (consecutive == null)
        {
            return;
        }

        var oldPositions = leaderboard.GetPositions();

        if (leaderboard.AddFastestConsecutiveLap(pilotId, consecutive.StartLapNumber, consecutive.EndLapNumber, consecutive.TotalLaps, consecutive.TotalMilliseconds, consecutive.LastLapCompletionUtc))
        {
            var newPositions = leaderboard.GetPositions();
            await _publishSubscribe.Dispatch(new OpenPracticeConsecutiveLapLeaderboardPositionsChanged(sessionId, leaderboard.LeaderboardId,
                               oldPositions.Select(x => new OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds)),
                               newPositions.Select(x => new OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds))));
        }
    }
}
