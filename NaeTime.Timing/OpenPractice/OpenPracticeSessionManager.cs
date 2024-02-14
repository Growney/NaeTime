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
    public async Task When(OpenPracticeConsecutiveLapLeaderboardConfigured leaderboard)
    {
        var sessionResponse = await _publishSubscribe.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(leaderboard.SessionId));

        if (sessionResponse == null)
        {
            return;
        }

        var consecutiveLapLeaderboard = new ConsecutiveLapsLeaderboard(leaderboard.LeaderboardId, leaderboard.ConsecutiveLaps);

        var groupedLaps = sessionResponse.Laps.GroupBy(x => x.PilotId);
        var consecutiveLapCalculator = new FastestConsecutiveLapCalculator();
        foreach (var group in groupedLaps)
        {
            var fastestConsecutive = consecutiveLapCalculator.CalculateFastestConsecutiveLaps(consecutiveLapLeaderboard.LapCount, group.Select(x => new Lap(x.Id, x.LapNumber, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   OpenPracticeSessionResponse.LapStatus.Invalid => LapStatus.Invalid,
                                   OpenPracticeSessionResponse.LapStatus.Completed => LapStatus.Completed,
                                   _ => throw new NotImplementedException()
                               }, x.TotalMilliseconds)));

            consecutiveLapLeaderboard.UpdateIfFaster(group.Key, fastestConsecutive.StartLapNumber, fastestConsecutive.EndLapNumber, fastestConsecutive.TotalLaps, fastestConsecutive.TotalMilliseconds, fastestConsecutive.LastLapCompletionUtc);
        }
        var positions = consecutiveLapLeaderboard.GetPositions();
        await _publishSubscribe.Dispatch(new OpenPracticeConsecutiveLapLeaderboardPositionsChanged(leaderboard.SessionId, leaderboard.LeaderboardId,
                               Enumerable.Empty<OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition>(),
                               positions.Select(x => new OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletion))));
    }
    public async Task When(OpenPracticeSingleLapLeaderboardConfigured leaderboard)
    {
        var sessionResponse = await _publishSubscribe.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(leaderboard.SessionId));

        if (sessionResponse == null)
        {
            return;
        }

        var consecutiveLapLeaderboard = new SingleLapLeaderboard(leaderboard.LeaderboardId);

        var groupedLaps = sessionResponse.Laps.GroupBy(x => x.PilotId);
        var consecutiveLapCalculator = new FastestSingleLapCalculator();
        foreach (var group in groupedLaps)
        {
            var fastestConsecutive = consecutiveLapCalculator.Calculate(group.Select(x => new Lap(x.Id, x.LapNumber, x.StartedUtc, x.FinishedUtc,
                               x.Status switch
                               {
                                   OpenPracticeSessionResponse.LapStatus.Invalid => LapStatus.Invalid,
                                   OpenPracticeSessionResponse.LapStatus.Completed => LapStatus.Completed,
                                   _ => throw new NotImplementedException()
                               }, x.TotalMilliseconds)));

            if (fastestConsecutive == null)
            {
                continue;
            }
            consecutiveLapLeaderboard.UpdateIfFaster(group.Key, fastestConsecutive.LapNumber, fastestConsecutive.LapMilliseconds, fastestConsecutive.CompletionUtc);
        }
        var positions = consecutiveLapLeaderboard.GetPositions();
        await _publishSubscribe.Dispatch(new OpenPracticeSingleLapLeaderboardPositionsChanged(leaderboard.SessionId, leaderboard.LeaderboardId,
                               Enumerable.Empty<OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition>(),
                               positions.Select(x => new OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds, x.CompletionTime))));
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

        var newLapId = Guid.NewGuid();

        await _publishSubscribe.Dispatch(new OpenPracticeLapCompleted(newLapId, lapCompleted.SessionId, pilotLane.PilotId, lapCompleted.LapNumber, lapCompleted.StartedUtcTime, lapCompleted.FinishedUtcTime, lapCompleted.TotalTime));

        var singleLapLeaderboards = BuildSingleLapLeaderboards(sessionResponse.SingleLapLeaderboards);
        var consecutiveLapLeaderboards = BuildConsecutiveLapLeaderboard(sessionResponse.ConsecutiveLapLeaderboards);

        if (singleLapLeaderboards.Any() || consecutiveLapLeaderboards.Any())
        {
            var pilotLaps = sessionResponse.Laps.Where(x => x.PilotId == pilotLane.PilotId).Select(x => new Lap(x.Id, x.LapNumber, x.StartedUtc, x.FinishedUtc,
                x.Status switch
                {
                    OpenPracticeSessionResponse.LapStatus.Invalid => LapStatus.Invalid,
                    OpenPracticeSessionResponse.LapStatus.Completed => LapStatus.Completed,
                    _ => throw new NotImplementedException()
                }, x.TotalMilliseconds)).ToList();

            pilotLaps.Add(new Lap(newLapId, lapCompleted.LapNumber, lapCompleted.StartedUtcTime, lapCompleted.FinishedUtcTime, LapStatus.Completed, lapCompleted.TotalTime));

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
    public async Task When(OpenPracticeLapRemoved removed)
    {
        var sessionResponse = await _publishSubscribe.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(removed.SessionId));

        if (sessionResponse == null)
        {
            return;
        }
        var singleLapLeaderboards = BuildSingleLapLeaderboards(sessionResponse.SingleLapLeaderboards);
        var consecutiveLapLeaderboards = BuildConsecutiveLapLeaderboard(sessionResponse.ConsecutiveLapLeaderboards);

        if (singleLapLeaderboards.Any() || consecutiveLapLeaderboards.Any())
        {
            var pilotLaps = sessionResponse.Laps.Where(x => x.PilotId == removed.PilotId && x.Id != removed.LapId).Select(x => new Lap(x.Id, x.LapNumber, x.StartedUtc, x.FinishedUtc,
                x.Status switch
                {
                    OpenPracticeSessionResponse.LapStatus.Invalid => LapStatus.Invalid,
                    OpenPracticeSessionResponse.LapStatus.Completed => LapStatus.Completed,
                    _ => throw new NotImplementedException()
                }, x.TotalMilliseconds)).ToList();


            foreach (var singlelapLeaderboard in singleLapLeaderboards)
            {
                await UpdateSingleLapLeaderboard(sessionResponse.SessionId, removed.PilotId, singlelapLeaderboard, pilotLaps);
            }
            foreach (var consecutiveLapLeaderboard in consecutiveLapLeaderboards)
            {
                await UpdateConsecutiveLapLeaderboards(sessionResponse.SessionId, removed.PilotId, consecutiveLapLeaderboard, pilotLaps);
            }
        }
    }
    public async Task When(LapInvalidated lapInvalidated)
    {
        var sessionResponse = await _publishSubscribe.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(lapInvalidated.SessionId));

        if (sessionResponse == null)
        {
            return;
        }

        var pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapInvalidated.Lane);
        if (pilotLane == null)
        {
            return;
        }

        await _publishSubscribe.Dispatch(new OpenPracticeLapInvalidated(Guid.NewGuid(), lapInvalidated.SessionId, pilotLane.PilotId, lapInvalidated.LapNumber, lapInvalidated.StartedUtcTime, lapInvalidated.FinishedUtcTime, lapInvalidated.TotalTime));
    }

    private IEnumerable<SingleLapLeaderboard> BuildSingleLapLeaderboards(IEnumerable<OpenPracticeSessionResponse.SingleLapLeaderboard> sessionResponse)
    {
        var singleLapLeaderboards = new List<SingleLapLeaderboard>();

        foreach (var singleLapLeaderboard in sessionResponse)
        {
            var leaderboard = new SingleLapLeaderboard(singleLapLeaderboard.LeaderboardId);
            foreach (var position in singleLapLeaderboard.Positions)
            {
                leaderboard.UpdateIfFaster(position.PilotId, position.LapNumber, position.TotalMilliseconds, position.CompletionUtc);
            }
            singleLapLeaderboards.Add(leaderboard);
        }

        return singleLapLeaderboards;
    }
    private IEnumerable<ConsecutiveLapsLeaderboard> BuildConsecutiveLapLeaderboard(IEnumerable<OpenPracticeSessionResponse.ConsecutiveLapLeaderboard> sessionResponse)
    {
        var consecutiveLapLeaderboards = new List<ConsecutiveLapsLeaderboard>();

        foreach (var consecutiveLapLeaderboard in sessionResponse)
        {
            var leaderboard = new ConsecutiveLapsLeaderboard(consecutiveLapLeaderboard.LeaderboardId, consecutiveLapLeaderboard.ConsecutiveLaps);
            foreach (var position in consecutiveLapLeaderboard.Positions)
            {
                leaderboard.UpdateIfFaster(position.PilotId, position.StartLapNumber, position.EndLapNumber, position.TotalLaps, position.TotalMilliseconds, position.LastLapCompletionUtc);
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

        if (leaderboard.UpdateIfFaster(pilotId, fastestSingle.LapNumber, fastestSingle.LapMilliseconds, fastestSingle.CompletionUtc))
        {
            await DispatchUpdatedSingleLeaderboardPositions(sessionId, leaderboard, oldPositions);
        }
    }
    private async Task UpdateSingleLapLeaderboard(Guid sessionId, Guid pilotId, SingleLapLeaderboard leaderboard, IEnumerable<Lap> laps)
    {
        var calculator = new FastestSingleLapCalculator();
        var fastestSingle = calculator.Calculate(laps);

        var oldPositions = leaderboard.GetPositions();

        if (fastestSingle == null)
        {
            leaderboard.RemovePilot(pilotId);
            await DispatchUpdatedSingleLeaderboardPositions(sessionId, leaderboard, oldPositions);
        }
        else
        {
            if (leaderboard.SetFastest(pilotId, fastestSingle.LapNumber, fastestSingle.LapMilliseconds, fastestSingle.CompletionUtc))
            {
                await DispatchUpdatedSingleLeaderboardPositions(sessionId, leaderboard, oldPositions);
            }
        }

    }
    private async Task DispatchUpdatedSingleLeaderboardPositions(Guid sessionId, SingleLapLeaderboard leaderboard, IEnumerable<SingleLapLeaderboardPosition> oldPositions)
    {
        var newPositions = leaderboard.GetPositions();
        await _publishSubscribe.Dispatch(new OpenPracticeSingleLapLeaderboardPositionsChanged(sessionId, leaderboard.LeaderboardId,
            oldPositions.Select(x => new OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds, x.CompletionTime)),
            newPositions.Select(x => new OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds, x.CompletionTime))));
    }
    private async Task CheckConsecutiveLapLeaderboards(Guid sessionId, Guid pilotId, ConsecutiveLapsLeaderboard leaderboard, IEnumerable<Lap> laps)
    {
        var calculator = new FastestConsecutiveLapCalculator();
        var consecutive = calculator.CalculateFastestConsecutiveLaps(leaderboard.LapCount, laps);

        if (consecutive == null)
        {
            return;
        }

        var oldPositions = leaderboard.GetPositions();

        if (leaderboard.UpdateIfFaster(pilotId, consecutive.StartLapNumber, consecutive.EndLapNumber, consecutive.TotalLaps, consecutive.TotalMilliseconds, consecutive.LastLapCompletionUtc))
        {
            await DispatchUpdatedConsecutiveLeaderboardPositions(sessionId, leaderboard, oldPositions);
        }
    }
    private async Task DispatchUpdatedConsecutiveLeaderboardPositions(Guid sessionId, ConsecutiveLapsLeaderboard leaderboard, IEnumerable<ConsecutiveLapsLeaderboardPosition> oldPositions)
    {
        var newPositions = leaderboard.GetPositions();
        await _publishSubscribe.Dispatch(new OpenPracticeConsecutiveLapLeaderboardPositionsChanged(sessionId, leaderboard.LeaderboardId,
                           oldPositions.Select(x => new OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletion)),
                           newPositions.Select(x => new OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletion))));
    }
    private async Task UpdateConsecutiveLapLeaderboards(Guid sessionId, Guid pilotId, ConsecutiveLapsLeaderboard leaderboard, IEnumerable<Lap> laps)
    {
        var calculator = new FastestConsecutiveLapCalculator();
        var consecutive = calculator.CalculateFastestConsecutiveLaps(leaderboard.LapCount, laps);

        var oldPositions = leaderboard.GetPositions();
        if (consecutive == null)
        {
            leaderboard.RemovePilot(pilotId);
            await DispatchUpdatedConsecutiveLeaderboardPositions(sessionId, leaderboard, oldPositions);
            return;
        }
        else
        {
            if (leaderboard.SetFastest(pilotId, consecutive.StartLapNumber, consecutive.EndLapNumber, consecutive.TotalLaps, consecutive.TotalMilliseconds, consecutive.LastLapCompletionUtc))
            {
                await DispatchUpdatedConsecutiveLeaderboardPositions(sessionId, leaderboard, oldPositions);
            }
        }

    }
}
