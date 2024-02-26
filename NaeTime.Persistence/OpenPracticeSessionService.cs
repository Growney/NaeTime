using NaeTime.Messages.Events.OpenPractice;
using NaeTime.Messages.Requests.OpenPractice;
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
    public async Task When(OpenPracticeConsecutiveLapCountTracked tracked)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        await repository.AddTrackedConsecutiveLaps(tracked.SessionId, tracked.LapCap).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeConsecutiveLapCountTrackingRemoved removed)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        await repository.RemoveTrackedConsecutiveLaps(removed.SessionId, removed.LapCap).ConfigureAwait(false);

    }

    public async Task<OpenPracticeSessionResponse?> On(OpenPracticeSessionRequest request)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        var session = await repository.Get(request.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }

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
            session.TrackedConsecutiveLaps);

    }
    public async Task<OpenPracticeSessionTrackedConsecutiveLapsResponse> On(OpenPracticeSessionTrackedConsecutiveLapsRequest request)
    {
        var repository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);

        throw new NotImplementedException();

    }
}

