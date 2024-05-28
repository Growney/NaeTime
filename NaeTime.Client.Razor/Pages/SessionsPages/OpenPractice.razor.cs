using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Management.Messages;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.SessionsPages;
public partial class OpenPractice : ComponentBase, IDisposable
{
    [Inject]
    public IRemoteProcedureCallClient RpcClient { get; set; } = default!;
    [Inject]
    public IEventClient EventClient { get; set; } = default!;
    [Inject]
    public IEventRegistrarScope EventRegistrarScope { get; set; } = default!;

    private readonly List<LaneConfiguration> _laneConfigurations = new();
    private readonly List<TrackDetails> _tracks = new();
    private readonly List<Pilot> _pilots = new();
    private readonly List<SessionDetails> _sessionDetails = new();

    private OpenPracticeSession? _selectedSession;
    private Guid? _activeSessionId;
    private bool _isLaneConfigCollapsed = false;
    protected override async Task OnInitializedAsync()
    {
        EventRegistrarScope.RegisterHub(this);

        IEnumerable<Management.Messages.Models.Pilot>? pilotsResponse = await RpcClient.InvokeAsync<IEnumerable<Management.Messages.Models.Pilot>>("GetPilots");

        if (pilotsResponse != null)
        {
            _pilots.AddRange(pilotsResponse.Select(x => new Pilot()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                CallSign = x.CallSign
            }));
        }

        IEnumerable<Management.Messages.Models.Track>? tracksResponse = await RpcClient.InvokeAsync<IEnumerable<Management.Messages.Models.Track>>("GetTracks");

        if (tracksResponse != null)
        {
            _tracks.AddRange(tracksResponse.Select(x => new TrackDetails(x.Id, x.Name, x.MinimumLapTimeMilliseconds, x.MaximumLapTimeMilliseconds, x.Timers, x.AllowedLanes)));
        }

        IEnumerable<Timing.Messages.Models.ActiveLaneConfiguration>? activeLaneConfigurations = await RpcClient.InvokeAsync<IEnumerable<Timing.Messages.Models.ActiveLaneConfiguration>>("GetActiveLaneConfigurations");

        if (activeLaneConfigurations != null)
        {
            _laneConfigurations.AddRange(activeLaneConfigurations.Select(x => new LaneConfiguration()
            {
                LaneNumber = x.Lane,
                BandId = x.BandId,
                FrequencyInMhz = x.FrequencyInMhz,
                IsEnabled = x.IsEnabled
            }));
        }

        IEnumerable<NaeTime.OpenPractice.Messages.Models.OpenPracticeSession>? sessions = await RpcClient.InvokeAsync<IEnumerable<NaeTime.OpenPractice.Messages.Models.OpenPracticeSession>>("GetOpenPracticeSessions");
        if (sessions != null)
        {
            _sessionDetails.AddRange(sessions.Select(x => new SessionDetails
            {
                Id = x.Id,
                Name = x.Name,
                Type = SessionType.OpenPractice
            }));
        }


        Management.Messages.Models.ActiveSession? activeSessionReponse = await RpcClient.InvokeAsync<Management.Messages.Models.ActiveSession?>("GetActiveSession");

        if (activeSessionReponse != null)
        {
            _activeSessionId = activeSessionReponse.SessionId;
            await SetupForSession(activeSessionReponse.SessionId);
        }

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

        NaeTime.OpenPractice.Messages.Models.OpenPracticeSession? practiceSessionResponse = await RpcClient.InvokeAsync<NaeTime.OpenPractice.Messages.Models.OpenPracticeSession?>("GetOpenPracticeSession", sessionId);
        if (practiceSessionResponse == null)
        {
            return;
        }

        Management.Messages.Models.Track? track = await RpcClient.InvokeAsync<Management.Messages.Models.Track?>("GetTrack", practiceSessionResponse.TrackId);

        if (track == null)
        {
            return;
        }

        IEnumerable<Timing.Messages.Models.LaneActiveTimings>? timings = await RpcClient.InvokeAsync<IEnumerable<Timing.Messages.Models.LaneActiveTimings>>("GetSessionActiveTimings", sessionId);

        byte allowedLanes = track.AllowedLanes;

        _selectedSession = new OpenPracticeSession
        {
            Id = practiceSessionResponse.Id,
            TrackId = practiceSessionResponse.TrackId,
            Name = practiceSessionResponse.Name,
            MinimumLapMilliseconds = practiceSessionResponse.MinimumLapMilliseconds,
            MaximumLapMilliseconds = practiceSessionResponse.MaximumLapMilliseconds,
            Lanes = GetLaneConfigurations(allowedLanes, practiceSessionResponse.ActiveLanes, timings).ToList(),
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
                    NaeTime.OpenPractice.Messages.Models.LapStatus.Completed => OpenPracticeLapStatus.Completed,
                    NaeTime.OpenPractice.Messages.Models.LapStatus.Invalid => OpenPracticeLapStatus.Invalid,
                    _ => throw new NotImplementedException()
                }
            }).ToList(),
            TrackedConsecutiveLaps = practiceSessionResponse.TrackedConsecutiveLaps.ToList()
        };

    }
    private IEnumerable<OpenPracticeLaneConfiguration> GetLaneConfigurations(byte allowedLanes, IEnumerable<NaeTime.OpenPractice.Messages.Models.PilotLane> lanes, IEnumerable<Timing.Messages.Models.LaneActiveTimings>? timings)
    {
        for (byte lane = 1; lane <= allowedLanes; lane++)
        {
            LaneConfiguration? laneConfig = _laneConfigurations.FirstOrDefault(x => x.LaneNumber == lane);
            NaeTime.OpenPractice.Messages.Models.PilotLane? pilotLaneConfig = lanes.FirstOrDefault(x => x.Lane == lane);

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

        string sessionName = $"Quick Session - {DateTime.Now.ToShortDateString()} - {DateTime.Now.ToShortTimeString()}";
        await EventClient.Publish(new OpenPracticeSessionConfigured(sessionId, sessionName, trackId, minimumLapMilliseconds, maximumLapMilliseconds));

        await EventClient.Publish(new SessionActivated(sessionId, SessionActivated.SessionType.OpenPractice));

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
        IEnumerable<Hardware.Messages.Models.TimerDetails>? timersResponse = await RpcClient.InvokeAsync<IEnumerable<Hardware.Messages.Models.TimerDetails>>("GetAllTimerDetails");

        if (timersResponse == null)
        {
            return;
        }

        if (!timersResponse.Any())
        {
            return;
        }

        Guid newTrackId = Guid.NewGuid();
        IEnumerable<Hardware.Messages.Models.TimerDetails> trackTimers = timersResponse.Take(1);
        byte maxLanes = trackTimers.Max(x => x.MaxLanes);
        IEnumerable<Guid> timerIds = trackTimers.Select(x => x.Id);

        await EventClient.Publish(new TrackCreated(newTrackId, $"Quick Track -{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", 0, null, timerIds, maxLanes));

        Management.Messages.Models.Track? track = await RpcClient.InvokeAsync<Management.Messages.Models.Track?>("GetTrack", newTrackId);

        if (track == null)
        {
            return;
        }

        _tracks.Add(new TrackDetails(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers, track.AllowedLanes));

        Guid sessionId = Guid.NewGuid();

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

        return EventClient.Publish(new OpenPracticeMinimumLapTimeConfigured(_selectedSession.Id, minimumLapMilliseconds));
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

        return EventClient.Publish(new OpenPracticeMaximumLapTimeConfigured(_selectedSession.Id, maximumLapMilliseconds));
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
        return EventClient.Publish(new ConsecutiveLapCountTracked(sessionId, lapCap));
    }
    public void Dispose() => EventRegistrarScope?.Dispose();
}
