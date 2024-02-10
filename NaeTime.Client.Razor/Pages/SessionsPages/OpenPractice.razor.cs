using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Messages.Events.Activation;
using NaeTime.Messages.Events.Entities;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.SessionsPages;
public partial class OpenPractice : ComponentBase
{
    [Inject]
    public IDispatcher Dispatcher { get; set; } = default!;

    private byte _activeSessionLanes = 0;
    private readonly List<LaneConfiguration> _laneConfigurations = new();
    private readonly List<TrackDetails> _tracks = new();

    private Guid? _activeSession;
    protected override async Task OnInitializedAsync()
    {
        var tracksResponse = await Dispatcher.Request<TracksRequest, TracksResponse>();

        if (tracksResponse != null)
        {
            _tracks.AddRange(tracksResponse.Tracks.Select(x => new TrackDetails(x.Id, x.Name, x.MinimumLapTimeMilliseconds, x.MaximumLapTimeMilliseconds, x.Timers, x.AllowedLanes)));
        }

        var activeSessionReponse = await Dispatcher.Request<ActiveSessionRequest, ActiveSessionResponse>();

        if (activeSessionReponse != null)
        {
            _activeSessionLanes = activeSessionReponse.ActiveTrack.Lanes;
            _activeSession = activeSessionReponse.SessionId;
        }

        var activeLaneConfigurations = await Dispatcher.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();

        if (activeLaneConfigurations != null)
        {
            _laneConfigurations.AddRange(activeLaneConfigurations.Lanes.Select(x => new LaneConfiguration()
            {
                LaneNumber = x.Lane,
                BandId = x.BandId,
                FrequencyInMhz = x.FrequencyInMhz,
                IsEnabled = x.IsEnabled
            }));
        }

        if (_laneConfigurations.Count != _activeSessionLanes)
        {
            for (byte i = 1; i <= _activeSessionLanes; i++)
            {
                if (_laneConfigurations.Any(x => x.LaneNumber == i))
                {
                    continue;
                }
                _laneConfigurations.Add(new LaneConfiguration() { LaneNumber = i });
            }
        }

    }

    public Task StartSessionOnTrack(Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds)
    {
        return Dispatcher.Dispatch(new SessionActivated(Guid.NewGuid(), SessionActivated.SessionType.OpenPractice, trackId, minimumLapMilliseconds, maximumLapMilliseconds));
    }
    public async Task StartSessionOnNewTrack()
    {
        var timersResponse = await Dispatcher.Request<TimerDetailsRequest, TimerDetailsResponse>();

        if (timersResponse == null)
        {
            return;
        }

        if (!timersResponse.Timers.Any())
        {
            return;
        }

        var newTrackId = Guid.NewGuid();

        await Dispatcher.Dispatch(new TrackCreated(newTrackId, "New Track", 0, null, timersResponse.Timers.Take(1).Select(x => x.Id)));

        await StartSessionOnTrack(newTrackId, 0, null);
    }

}
