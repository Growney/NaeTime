using NaeTime.Messages.Events.Activation;
using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class ActiveService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public ActiveService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }
    public async Task When(SessionActivated activated)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.ActivateSession(activated.SessionId,
            activated.Type switch
            {
                SessionActivated.SessionType.OpenPractice => SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            }).ConfigureAwait(false);
    }
    public async Task When(SessionDeactivated deactivated)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.DeactivateSession().ConfigureAwait(false);
    }
    public async Task When(LapCompleted completed)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.DeactivateLap(completed.SessionId, completed.Lane).ConfigureAwait(false);
    }
    public async Task When(LapInvalidated invalidated)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.DeactivateLap(invalidated.SessionId, invalidated.Lane).ConfigureAwait(false);
    }
    public async Task When(LapStarted started)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.ActivateLap(started.SessionId, started.Lane, started.LapNumber, started.StartedSoftwareTime, started.StartedUtcTime, started.StartedHardwareTime).ConfigureAwait(false);
    }
    public async Task When(SplitCompleted completed)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.DeactivateSplit(completed.SessionId, completed.Lane).ConfigureAwait(false);
    }
    public async Task When(SplitStarted started)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.ActivateSplit(started.SessionId, started.Lane, started.LapNumber, started.Split, started.StartedSoftwareTime, started.StartedUtcTime).ConfigureAwait(false);
    }
    public async Task When(SplitSkipped skipped)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        await repository.DeactivateSplit(skipped.SessionId, skipped.Lane).ConfigureAwait(false);
    }

    public async Task<ActiveSessionResponse?> On(ActiveSessionRequest request)
    {
        var repository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        var session = await repository.GetSession().ConfigureAwait(false);

        return session == null
            ? null
            : session.Type switch
            {
                Models.SessionType.OpenPractice => await CreateOpenPracticeActiveSession(session.SessionId).ConfigureAwait(false),
                _ => throw new NotImplementedException(),
            };
    }

    private async Task<ActiveSessionResponse?> CreateOpenPracticeActiveSession(Guid sessionId)
    {
        var sessionRepository = await _repositoryFactory.CreateOpenPracticeSessionRepository().ConfigureAwait(false);
        if (sessionRepository == null)
        {
            return null;
        }

        var session = await sessionRepository.Get(sessionId).ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }

        var trackRepository = await _repositoryFactory.CreateTrackRepository().ConfigureAwait(false);

        var track = await trackRepository.Get(session.TrackId);

        if (track == null)
        {
            return null;
        }

        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);

        var timers = await hardwareRepository.GetTimerDetails(track.Timers);

        var maxLanes = timers.Min(x => x.AllowedLanes);

        var activeTrack = new ActiveSessionResponse.Track(track.Id, maxLanes, track.Timers);

        return new ActiveSessionResponse(session.SessionId, ActiveSessionResponse.SessionType.OpenPractice, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, activeTrack);
    }
    public async Task<ActiveTimingResponse?> On(ActiveTimingRequest request)
    {
        var activeRepository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        var timings = await activeRepository.GetTimings(request.SessionId, request.Lane);

        if (timings == null)
        {
            return new ActiveTimingResponse(request.SessionId, request.Lane, 0, null, null);
        }

        ActiveTimingResponse.ActiveLap? activeLap = null;
        if (timings.Lap != null)
        {
            var lap = timings.Lap;
            activeLap = new ActiveTimingResponse.ActiveLap(lap.StartedSoftwareTime, lap.StartedUtcTime, lap.StartedHardwareTime);
        }

        ActiveTimingResponse.ActiveSplit? activeSplit = null;
        if (timings.Split != null)
        {
            var split = timings.Split;
            activeSplit = new ActiveTimingResponse.ActiveSplit(split.SplitNumber, split.StartedSoftwareTime, split.StartedUtcTime);
        }

        return new ActiveTimingResponse(request.SessionId, request.Lane, timings.LapNumber, activeLap, activeSplit);
    }
    public async Task<ActiveTimingsResponse?> On(ActiveTimingsRequest request)
    {
        var activeRepository = await _repositoryFactory.CreateActiveRepository().ConfigureAwait(false);

        var timings = await activeRepository.GetTimings(request.SessionId).ConfigureAwait(false);

        if (timings == null)
        {
            return new ActiveTimingsResponse(request.SessionId, Enumerable.Empty<ActiveTimingsResponse.ActiveTimings>());
        }

        var responseTimings = timings.Select(x => new ActiveTimingsResponse.ActiveTimings(x.Lane, x.LapNumber,
                CreateActiveLap(x.Lap), CreateActiveSplit(x.Split)));

        return new ActiveTimingsResponse(request.SessionId, responseTimings);
    }
    private ActiveTimingsResponse.ActiveLap? CreateActiveLap(ActiveLap? lap) => lap == null ? null : new ActiveTimingsResponse.ActiveLap(lap.StartedSoftwareTime, lap.StartedUtcTime, lap.StartedHardwareTime);
    private ActiveTimingsResponse.ActiveSplit? CreateActiveSplit(ActiveSplit? split) => split == null
            ? null
            : new ActiveTimingsResponse.ActiveSplit(split.SplitNumber, split.StartedSoftwareTime, split.StartedUtcTime);
}
