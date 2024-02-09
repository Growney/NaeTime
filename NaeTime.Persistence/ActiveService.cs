using NaeTime.Messages.Events.Activation;
using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
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

        await repository.ActivateSession(activated.SessionId, activated.Type switch
        {
            SessionActivated.SessionType.OpenPractice => Models.SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        }, activated.TrackId, activated.MinimumLapMilliseconds, activated.MaximumLapMilliseconds);
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

        var sessionType = session.Type switch
        {
            Models.SessionType.OpenPractice => ActiveSessionResponse.SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        };

        return new ActiveSessionResponse(session.SessionId, sessionType, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, activeTrack);
    }
    public async Task<ActiveTimingsResponse?> On(ActiveTimingRequest request)
    {
        var activeRepository = await _repositoryFactory.CreateActiveRepository();

        var timings = await activeRepository.GetTimings(request.SessionId, request.Lane);

        if (timings == null)
        {
            return new ActiveTimingsResponse(request.SessionId, request.Lane, null, null);
        }
        ActiveTimingsResponse.ActiveLap? activeLap = null;
        if (timings.Lap != null)
        {
            var lap = timings.Lap;
            activeLap = new ActiveTimingsResponse.ActiveLap(lap.LapNumber, lap.StartedSoftwareTime, lap.StartedUtcTime, lap.StartedHardwareTime);
        }

        ActiveTimingsResponse.ActiveSplit? activeSplit = null;
        if (timings.Split != null)
        {
            var split = timings.Split;
            activeSplit = new ActiveTimingsResponse.ActiveSplit(split.LapNumber, split.SplitNumber, split.StartedSoftwareTime, split.StartedUtcTime);
        }


        return new ActiveTimingsResponse(request.SessionId, request.Lane, activeLap, activeSplit);
    }
    public async Task<ActiveLaneConfigurationResponse?> On(ActiveLaneConfigurationRequest request)
    {
        var activeRepository = await _repositoryFactory.CreateActiveRepository();

        var lanes = await activeRepository.GetLanes();

        return new ActiveLaneConfigurationResponse(lanes.Select(x => new ActiveLaneConfigurationResponse.LaneConfiguration(x.LaneNumber, x.Pilot, x.FrequencyInMhz, x.IsEnabled)));
    }

}
