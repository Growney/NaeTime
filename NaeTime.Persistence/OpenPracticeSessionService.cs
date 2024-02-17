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
    public async Task When(OpenPracticeLapDisputed lap)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        switch (lap.ActualStatus)
        {
            case OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid:
                await repository.SetLapStatus(lap.LapId, Models.OpenPracticeLapStatus.Invalid);
                break;
            case OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed:
                await repository.SetLapStatus(lap.LapId, Models.OpenPracticeLapStatus.Completed);
                break;
            default:
                break;
        }
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        await repository.RemoveLap(removed.SessionId, removed.LapId).ConfigureAwait(false);

    }
    public async Task When(OpenPracticeSessionConfigured configured)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        await repository.AddOrUpdate(configured.SessionId, configured.Name, configured.TrackId, configured.MinimumLapMilliseconds, configured.MaximumLapMilliseconds).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeMaximumLapTimeConfigured configured)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        await repository.SetMaximumLap(configured.SessionId, configured.MaximumLapMilliseconds).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeMinimumLapTimeConfigured configured)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        await repository.SetMinimumLap(configured.SessionId, configured.MinimumLapMilliseconds).ConfigureAwait(false);
    }
    public async Task<OpenPracticeSessionResponse?> On(OpenPracticeSessionRequest request)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(request.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }
        var singleLapLeaderboards = session.SingleLapLeaderboards.Select(x => new OpenPracticeSessionResponse.SingleLapLeaderboard(x.LeaderboardId,
            x.Positions.Select(y => new OpenPracticeSessionResponse.SingleLapLeaderboardPosition(y.Position, y.PilotId, y.LapId, y.LapMilliseconds, y.CompletionUtc))));
        var consecutiveLapLeaderboards = session.ConsecutiveLapLeaderboards.Select(x => new OpenPracticeSessionResponse.ConsecutiveLapLeaderboard(x.LeaderboardId, x.ConsecutiveLaps,
            x.Positions.Select(y => new OpenPracticeSessionResponse.ConsecutiveLapLeaderboardPosition(y.Position, y.PilotId, y.TotalLaps, y.TotalMilliseconds, y.LastLapCompletionUtc, y.IncludedLaps))));
        var laps = session.Laps.Select(x => new OpenPracticeSessionResponse.Lap(x.LapId, x.PilotId, x.StartedUtc, x.FinishedUtc,
            x.Status switch
            {
                Models.OpenPracticeLapStatus.Invalid => OpenPracticeSessionResponse.LapStatus.Invalid,
                Models.OpenPracticeLapStatus.Completed => OpenPracticeSessionResponse.LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, x.TotalMilliseconds));
        var lanes = session.ActiveLanes.Select(x => new OpenPracticeSessionResponse.PilotLane(x.PilotId, x.Lane));

        return new OpenPracticeSessionResponse(session.SessionId, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds,
            laps,
            lanes,
            singleLapLeaderboards,
            consecutiveLapLeaderboards);

    }
    public async Task When(OpenPracticeLapCompleted openPracticeLapCompleted)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(openPracticeLapCompleted.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        await repository.AddLapToSession(openPracticeLapCompleted.SessionId, openPracticeLapCompleted.LapId, openPracticeLapCompleted.PilotId, Models.OpenPracticeLapStatus.Completed, openPracticeLapCompleted.StartedUtc, openPracticeLapCompleted.FinishedUtc, openPracticeLapCompleted.TotalMilliseconds).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapInvalidated openPracticeLapInvalidated)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(openPracticeLapInvalidated.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        await repository.AddLapToSession(openPracticeLapInvalidated.SessionId, openPracticeLapInvalidated.LapId, openPracticeLapInvalidated.PilotId, Models.OpenPracticeLapStatus.Invalid, openPracticeLapInvalidated.StartedUtc, openPracticeLapInvalidated.FinishedUtc, openPracticeLapInvalidated.TotalMilliseconds).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLanePilotSet laneSet)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(laneSet.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        await repository.SetSessionLanePilot(laneSet.SessionId, laneSet.Lane, laneSet.PilotId).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeConsecutiveLapLeaderboardPositionsChanged newPositions)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var positions = newPositions.NewPositions.Select(x => new Models.ConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletion, x.IncludedLaps));

        await repository.UpdateConsecutiveLapsLeaderboardPositions(newPositions.SessionId, newPositions.LeaderboardId, positions).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeSingleLapLeaderboardPositionsChanged newPositions)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(newPositions.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        var positions = newPositions.NewPositions.Select(x => new Models.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.LapId, x.LapMilliseconds, x.CompletionUtc));

        await repository.UpdateSingleLapLeaderboard(newPositions.SessionId, newPositions.LeaderboardId, positions).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeConsecutiveLapLeaderboardConfigured leaderboard)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(leaderboard.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        await repository.AddOrUpdateConsecutiveLapsLeaderboard(leaderboard.SessionId, leaderboard.LeaderboardId, leaderboard.ConsecutiveLaps).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeSingleLapLeaderboardConfigured leaderboard)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(leaderboard.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        await repository.AddOrUpdateSingleLapLeaderboard(leaderboard.SessionId, leaderboard.LeaderboardId).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLeaderboardRemoved removed)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(removed.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }
        await repository.RemoveLeaderboard(removed.SessionId, removed.LeaderboardId).ConfigureAwait(false);
    }
}

