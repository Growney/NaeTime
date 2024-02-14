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
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.ActivateSession(activated.SessionId,
            activated.Type switch
            {
                SessionActivated.SessionType.OpenPractice => SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            });
    }
    public async Task When(SessionDeactivated deactivated)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.DeactivateSession();
    }
    public async Task When(LapCompleted completed)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.DeactivateLap(completed.SessionId, completed.Lane);
    }
    public async Task When(LapInvalidated invalidated)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.DeactivateLap(invalidated.SessionId, invalidated.Lane);
    }
    public async Task When(LapStarted started)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.ActivateLap(started.SessionId, started.Lane, started.LapNumber, started.StartedSoftwareTime, started.StartedUtcTime, started.StartedHardwareTime);
    }
    public async Task When(SplitCompleted completed)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.DeactivateSplit(completed.SessionId, completed.Lane);
    }
    public async Task When(SplitStarted started)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.ActivateSplit(started.SessionId, started.Lane, started.LapNumber, started.Split, started.StartedSoftwareTime, started.StartedUtcTime);
    }
    public async Task When(SplitSkipped skipped)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        await repository.DeactivateSplit(skipped.SessionId, skipped.Lane);
    }

    public async Task<ActiveSessionResponse?> On(ActiveSessionRequest request)
    {
        var repository = await _repositoryFactory.CreateActiveRepository();

        var session = await repository.GetSession();

        if (session == null)
        {
            return null;
        }

        switch (session.Type)
        {
            case Models.SessionType.OpenPractice:
                return await CreateOpenPracticeActiveSession(session.SessionId);
            default:
                throw new NotImplementedException();
        }
    }

    private async Task<ActiveSessionResponse?> CreateOpenPracticeActiveSession(Guid sessionId)
    {
        var sessionRepository = await _repositoryFactory.CreateOpenPracticeSessionRepository();
        if (sessionRepository == null)
        {
            return null;
        }
        var session = await sessionRepository.Get(sessionId);

        if (session == null)
        {
            return null;
        }

        var trackRepository = await _repositoryFactory.CreateTrackRepository();

        var track = await trackRepository.Get(session.TrackId);

        if (track == null)
        {
            return null;
        }
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository();

        var timers = await hardwareRepository.GetTimerDetails(track.Timers);

        var maxLanes = timers.Min(x => x.AllowedLanes);

        var activeTrack = new ActiveSessionResponse.Track(track.Id, maxLanes, track.Timers);

        return new ActiveSessionResponse(session.SessionId, ActiveSessionResponse.SessionType.OpenPractice, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, activeTrack);
    }
    public async Task<ActiveTimingResponse?> On(ActiveTimingRequest request)
    {
        var activeRepository = await _repositoryFactory.CreateActiveRepository();

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
        var activeRepository = await _repositoryFactory.CreateActiveRepository();

        var timings = await activeRepository.GetTimings(request.SessionId);

        if (timings == null)
        {
            return new ActiveTimingsResponse(request.SessionId, Enumerable.Empty<ActiveTimingsResponse.ActiveTimings>());
        }

        var responseTimings = timings.Select(x =>
        {
            return new ActiveTimingsResponse.ActiveTimings(x.Lane, x.LapNumber,
                CreateActiveLap(x.Lap), CreateActiveSplit(x.Split));
        });

        return new ActiveTimingsResponse(request.SessionId, responseTimings);
    }
    private ActiveTimingsResponse.ActiveLap? CreateActiveLap(ActiveLap? lap)
    {
        if (lap == null)
        {
            return null;
        }
        return new ActiveTimingsResponse.ActiveLap(lap.StartedSoftwareTime, lap.StartedUtcTime, lap.StartedHardwareTime);
    }
    private ActiveTimingsResponse.ActiveSplit? CreateActiveSplit(ActiveSplit? split)
    {
        if (split == null)
        {
            return null;
        }
        return new ActiveTimingsResponse.ActiveSplit(split.SplitNumber, split.StartedSoftwareTime, split.StartedUtcTime);
    }
}
