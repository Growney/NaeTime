using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class OpenPracticeSessionService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;
    public OpenPracticeSessionService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }
    public async Task When(OpenPracticeSessionConfigured configured)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        await repository.AddOrUpdate(configured.SessionId, configured.Name, configured.TrackId, configured.MinimumLapMilliseconds, configured.MaximumLapMilliseconds);
    }
    public async Task<OpenPracticeSessionResponse?> On(OpenPracticeSessionRequest request)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(request.SessionId);

        if (session == null)
        {
            return null;
        }
        var singleLapLeaderboards = session.SingleLapLeaderboards.Select(x => new OpenPracticeSessionResponse.SingleLapLeaderboard(x.LeaderboardId,
            x.Positions.Select(y => new OpenPracticeSessionResponse.SingleLapLeaderboardPosition(y.Position, y.PilotId, y.LapNumber, y.LapMilliseconds, y.CompletionUtc))));
        var consecutiveLapLeaderboards = session.ConsecutiveLapLeaderboards.Select(x => new OpenPracticeSessionResponse.ConsecutiveLapLeaderboard(x.LeaderboardId, x.ConsecutiveLaps,
            x.Positions.Select(y => new OpenPracticeSessionResponse.ConsecutiveLapLeaderboardPosition(y.Position, y.PilotId, y.StartLapNumber, y.EndLapNumber, y.TotalLaps, y.TotalMilliseconds, y.LastLapCompletionUtc))));
        var laps = session.Laps.Select(x => new OpenPracticeSessionResponse.Lap(x.LapId, x.PilotId, x.LapNumber, x.StartedUtc, x.FinishedUtc,
            x.Status switch
            {
                Models.OpenPracticeLapStatus.Invalid => OpenPracticeSessionResponse.LapStatus.Invalid,
                Models.OpenPracticeLapStatus.Completed => OpenPracticeSessionResponse.LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, x.TotalMilliseconds));
        var lanes = session.ActiveLanes.Select(x => new OpenPracticeSessionResponse.PilotLane(x.PilotId, x.Lane));

        return new OpenPracticeSessionResponse(session.SessionId, session.TrackId, session.Name,
            laps,
            lanes,
            singleLapLeaderboards,
            consecutiveLapLeaderboards);

    }

    public async Task When(OpenPracticeLapCompleted openPracticeLapCompleted)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(openPracticeLapCompleted.SessionId);

        if (session == null)
        {
            return;
        }

        await repository.AddLapToSession(openPracticeLapCompleted.SessionId, openPracticeLapCompleted.LapId, openPracticeLapCompleted.PilotId, openPracticeLapCompleted.LapNumber, Models.OpenPracticeLapStatus.Completed, openPracticeLapCompleted.StartedUtc, openPracticeLapCompleted.FinishedUtc, openPracticeLapCompleted.TotalMilliseconds);
    }
    public async Task When(OpenPracticeLapInvalidated openPracticeLapInvalidated)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(openPracticeLapInvalidated.SessionId);

        if (session == null)
        {
            return;
        }

        await repository.AddLapToSession(openPracticeLapInvalidated.SessionId, openPracticeLapInvalidated.LapId, openPracticeLapInvalidated.PilotId, openPracticeLapInvalidated.LapNumber, Models.OpenPracticeLapStatus.Invalid, openPracticeLapInvalidated.StartedUtc, openPracticeLapInvalidated.FinishedUtc, openPracticeLapInvalidated.TotalMilliseconds);
    }

    public async Task When(OpenPracticeLanePilotSet laneSet)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(laneSet.SessionId);

        if (session == null)
        {
            return;
        }

        await repository.SetSessionLanePilot(laneSet.SessionId, laneSet.Lane, laneSet.PilotId);
    }

    public async Task When(OpenPracticeConsecutiveLapLeaderboardPositionsChanged newPositions)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(newPositions.SessionId);

        if (session == null)
        {
            return;
        }

        var positions = newPositions.NewPositions.Select(x => new Models.ConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.StartLapNumber, x.EndLapNumber, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletion));

        await repository.UpdateConsecutiveLapsLeaderboardPositions(newPositions.SessionId, newPositions.LeaderboardId, positions);
    }

    public async Task When(OpenPracticeSingleLapLeaderboardPositionsChanged newPositions)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(newPositions.SessionId);

        if (session == null)
        {
            return;
        }

        var positions = newPositions.NewPositions.Select(x => new Models.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapNumber, x.LapMilliseconds, x.CompletionUtc));

        await repository.UpdateSingleLapLeaderboard(newPositions.SessionId, newPositions.LeaderboardId, positions);
    }

    public async Task When(OpenPracticeConsecutiveLapLeaderboardConfigured leaderboard)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(leaderboard.SessionId);

        if (session == null)
        {
            return;
        }

        await repository.AddOrUpdateConsecutiveLapsLeaderboard(leaderboard.SessionId, leaderboard.LeaderboardId, leaderboard.ConsecutiveLaps);
    }

    public async Task When(OpenPracticeSingleLapLeaderboardConfigured leaderboard)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(leaderboard.SessionId);

        if (session == null)
        {
            return;
        }

        await repository.AddOrUpdateSingleLapLeaderboard(leaderboard.SessionId, leaderboard.LeaderboardId);
    }
    public async Task When(OpenPracticeLeaderboardRemoved removed)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository();

        var session = await repository.Get(removed.SessionId);

        if (session == null)
        {
            return;
        }
        await repository.RemoveLeaderboard(removed.SessionId, removed.LeaderboardId);
    }
}

