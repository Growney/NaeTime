using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Hardware.Messages.Requests;
using NaeTime.Hardware.Messages.Responses;
using NaeTime.Management.Messages.Messages;
using NaeTime.Management.Messages.Requests;
using NaeTime.Management.Messages.Responses;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Requests;
using NaeTime.Timing.Messages.Responses;

namespace NaeTime.Client.Razor.Pages.SessionsPages;
public partial class OpenPractice : ComponentBase, IDisposable
{
    [Inject]
    public IPublishSubscribe PublishSubscribe { get; set; } = default!;

    private readonly List<LaneConfiguration> _laneConfigurations = new();
    private readonly List<TrackDetails> _tracks = new();
    private readonly List<Pilot> _pilots = new();
    private readonly List<SessionDetails> _sessionDetails = new();

    private OpenPracticeSession? _selectedSession;
    private Guid? _activeSessionId;
    protected override async Task OnInitializedAsync()
    {
        PublishSubscribe.Subscribe<OpenPracticeLapInvalidated>(this, When);
        PublishSubscribe.Subscribe<OpenPracticeLapRemoved>(this, When);
        PublishSubscribe.Subscribe<OpenPracticeLapCompleted>(this, When);
        PublishSubscribe.Subscribe<SessionActivated>(this, When);
        PublishSubscribe.Subscribe<SessionDeactivated>(this, When);
        var pilotsResponse = await PublishSubscribe.Request<PilotsRequest, PilotsResponse>();

        if (pilotsResponse != null)
        {
            _pilots.AddRange(pilotsResponse.Pilots.Select(x => new Pilot()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                CallSign = x.CallSign
            }));
        }
        var tracksResponse = await PublishSubscribe.Request<TracksRequest, TracksResponse>();

        if (tracksResponse != null)
        {
            _tracks.AddRange(tracksResponse.Tracks.Select(x => new TrackDetails(x.Id, x.Name, x.MinimumLapTimeMilliseconds, x.MaximumLapTimeMilliseconds, x.Timers, x.AllowedLanes)));
        }
        var activeLaneConfigurations = await PublishSubscribe.Request<ActiveLanesConfigurationRequest, ActiveLanesConfigurationResponse>();

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

        var sessions = await PublishSubscribe.Request<SessionsRequest, SessionsResponse>();
        if (sessions != null)
        {
            _sessionDetails.AddRange(sessions.Sessions.Where(x => x.Type == SessionsResponse.SessionType.OpenPractice).Select(x => new SessionDetails
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type switch
                {
                    SessionsResponse.SessionType.OpenPractice => SessionType.OpenPractice,
                    _ => throw new NotSupportedException()
                }
            }));
        }


        var activeSessionReponse = await PublishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>();

        if (activeSessionReponse != null)
        {
            _activeSessionId = activeSessionReponse.SessionId;
            await SetupForSession(activeSessionReponse.SessionId);
        }

    }

    public async Task When(OpenPracticeLapRemoved removed)
    {
        if (_selectedSession == null)
        {
            return;
        }

        if (removed.SessionId != _selectedSession.Id)
        {
            return;
        }

        var lapIndex = _selectedSession.Laps.FindIndex(x => x.Id == removed.LapId);

        if (lapIndex < 0)
        {
            return;
        }

        _selectedSession.Laps.RemoveAt(lapIndex);

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapCompleted lap)
    {
        if (_selectedSession == null)
        {
            return;
        }

        if (lap.SessionId != _selectedSession.Id)
        {
            return;
        }

        _selectedSession.Laps.Add(new OpenPracticeLap()
        {
            Id = lap.LapId,
            PilotId = lap.PilotId,
            PilotName = _pilots.FirstOrDefault(x => x.Id == lap.PilotId)?.CallSign,
            StartedUtc = lap.StartedUtc,
            FinishedUtc = lap.FinishedUtc,
            TotalMilliseconds = lap.TotalMilliseconds,
            Status = OpenPracticeLapStatus.Completed
        });
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapInvalidated lap)
    {
        if (_selectedSession == null)
        {
            return;
        }

        if (lap.SessionId != _selectedSession.Id)
        {
            return;
        }

        _selectedSession.Laps.Add(new OpenPracticeLap()
        {
            Id = lap.LapId,
            PilotId = lap.PilotId,
            PilotName = _pilots.FirstOrDefault(x => x.Id == lap.PilotId)?.CallSign,
            StartedUtc = lap.StartedUtc,
            FinishedUtc = lap.FinishedUtc,
            TotalMilliseconds = lap.TotalMilliseconds,
            Status = OpenPracticeLapStatus.Invalid
        });

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(SessionActivated session)
    {
        _activeSessionId = session.SessionId;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(SessionDeactivated session)
    {
        _activeSessionId = null;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    private async Task SetupForSession(Guid sessionId)
    {
        _selectedSession = null;

        var practiceSessionResponse = await PublishSubscribe.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(sessionId)).ConfigureAwait(false);
        if (practiceSessionResponse == null)
        {
            return;
        }

        var track = await PublishSubscribe.Request<TrackRequest, TrackResponse>(new TrackRequest(practiceSessionResponse.TrackId)).ConfigureAwait(false);

        if (track == null)
        {
            return;
        }


        var timings = await PublishSubscribe.Request<ActiveTimingsRequest, ActiveTimingsResponse>(new ActiveTimingsRequest(sessionId)).ConfigureAwait(false);


        var allowedLanes = track.AllowedLanes;

        _selectedSession = new OpenPracticeSession
        {
            Id = practiceSessionResponse.SessionId,
            TrackId = practiceSessionResponse.TrackId,
            Name = practiceSessionResponse.Name,
            MinimumLapMilliseconds = practiceSessionResponse.MinimumLapMilliseconds,
            MaximumLapMilliseconds = practiceSessionResponse.MaximumLapMilliseconds,
            Lanes = GetLaneConfigurations(allowedLanes, practiceSessionResponse.ActiveLanes, timings?.Timings).ToList(),
            Laps = practiceSessionResponse.Laps.Select(x => new OpenPracticeLap()
            {
                Id = x.Id,
                PilotId = x.PilotId,
                PilotName = _pilots.FirstOrDefault(y => y.Id == x.PilotId)?.CallSign,
                StartedUtc = x.StartedUtc,
                FinishedUtc = x.FinishedUtc,
                TotalMilliseconds = x.TotalMilliseconds,
                Status = x.Status switch
                {
                    OpenPracticeSessionResponse.LapStatus.Completed => OpenPracticeLapStatus.Completed,
                    OpenPracticeSessionResponse.LapStatus.Invalid => OpenPracticeLapStatus.Invalid,
                    _ => throw new NotImplementedException()
                }
            }).ToList(),
            TrackedConsecutiveLaps = practiceSessionResponse.TrackedConsecutiveLaps.ToList()
        };

    }
    private IEnumerable<OpenPracticeLaneConfiguration> GetLaneConfigurations(byte allowedLanes, IEnumerable<OpenPracticeSessionResponse.PilotLane> lanes, IEnumerable<ActiveTimingsResponse.ActiveTimings>? timings)
    {
        for (byte lane = 1; lane <= allowedLanes; lane++)
        {
            var laneConfig = _laneConfigurations.FirstOrDefault(x => x.LaneNumber == lane);
            var pilotLaneConfig = lanes.FirstOrDefault(x => x.Lane == lane);

            yield return new OpenPracticeLaneConfiguration()
            {
                LaneNumber = lane,
                BandId = laneConfig?.BandId,
                FrequencyInMhz = laneConfig?.FrequencyInMhz ?? 0,
                IsEnabled = laneConfig?.IsEnabled ?? false,
                PilotId = pilotLaneConfig?.PilotId,
                LapStarted = timings?.FirstOrDefault(x => x.Lane == lane)?.Lap?.StartedUtcTime
            };
        }
    }
    public string? GetActiveSessionName()
    {
        if (_selectedSession == null)
        {
            return "No Active Session";
        }

        return _selectedSession.Name;
    }
    public async Task StartNewSessionOnTrack(Guid sessionId, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds)
    {

        var sessionName = $"Quick Session - {DateTime.Now.ToShortDateString()} - {DateTime.Now.ToShortTimeString()}";
        await PublishSubscribe.Dispatch(new OpenPracticeSessionConfigured(sessionId, sessionName, trackId, minimumLapMilliseconds, maximumLapMilliseconds));

        await PublishSubscribe.Dispatch(new SessionActivated(sessionId, SessionActivated.SessionType.OpenPractice));

        _sessionDetails.Add(new SessionDetails
        {
            Id = sessionId,
            Name = sessionName,
            Type = SessionType.OpenPractice
        });

        await SetupForSession(sessionId);
    }
    public async Task StartSessionOnNewTrack()
    {
        var timersResponse = await PublishSubscribe.Request<TimerDetailsRequest, TimerDetailsResponse>();

        if (timersResponse == null)
        {
            return;
        }

        if (!timersResponse.Timers.Any())
        {
            return;
        }

        var newTrackId = Guid.NewGuid();
        var trackTimers = timersResponse.Timers.Take(1);
        var maxLanes = trackTimers.Max(x => x.MaxLanes);
        var timerIds = trackTimers.Select(x => x.Id);

        await PublishSubscribe.Dispatch(new TrackCreated(newTrackId, $"Quick Track -{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", 0, null, timerIds, maxLanes));

        var track = await PublishSubscribe.Request<TrackRequest, TrackResponse>(new TrackRequest(newTrackId)).ConfigureAwait(false);

        if (track == null)
        {
            return;
        }

        _tracks.Add(new TrackDetails(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers, track.AllowedLanes));

        var sessionId = Guid.NewGuid();

        await StartNewSessionOnTrack(sessionId, newTrackId, 0, null);
    }
    public Task SetSessionMinimumLapTime(long minimumLapMilliseconds)
    {
        if (_selectedSession == null)
        {
            return Task.CompletedTask;
        }

        if (_selectedSession.MinimumLapMilliseconds == minimumLapMilliseconds)
        {
            return Task.CompletedTask;
        }

        _selectedSession.MinimumLapMilliseconds = minimumLapMilliseconds;

        return PublishSubscribe.Dispatch(new OpenPracticeMinimumLapTimeConfigured(_selectedSession.Id, minimumLapMilliseconds));
    }
    public Task SetSessionMaximumLapTime(long? maximumLapMilliseconds)
    {
        if (_selectedSession == null)
        {
            return Task.CompletedTask;
        }

        if (_selectedSession.MaximumLapMilliseconds == maximumLapMilliseconds)
        {
            return Task.CompletedTask;
        }

        _selectedSession.MaximumLapMilliseconds = maximumLapMilliseconds;

        return PublishSubscribe.Dispatch(new OpenPracticeMaximumLapTimeConfigured(_selectedSession.Id, maximumLapMilliseconds));
    }
    public Task TrackConsecutiveLaps(Guid sessionId, uint lapCap)
    {
        if (_selectedSession == null)
        {
            return Task.CompletedTask;
        }

        if (_selectedSession.TrackedConsecutiveLaps.Any(x => x == lapCap))
        {
            return Task.CompletedTask;
        }

        _selectedSession.TrackedConsecutiveLaps.Add(lapCap);
        return PublishSubscribe.Dispatch(new ConsecutiveLapCountTracked(sessionId, lapCap));
    }
    public void Dispose() => PublishSubscribe.Unsubscribe(this);
}
