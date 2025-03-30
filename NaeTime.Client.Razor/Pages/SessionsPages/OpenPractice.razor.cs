using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Management;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Timing;
using NaeTime.PubSub.Abstractions;
using Syncfusion.Blazor.SplitButtons;

namespace NaeTime.Client.Razor.Pages.SessionsPages;
public partial class OpenPractice : ComponentBase, IDisposable
{
    [Inject]
    public INaeTimePersistence Persistence { get; set; } = default!;
    [Inject]
    public IDistributionReceiver Receiver { get; set; } = default!;
    [Inject]
    public IEventClient EventClient { get; set; } = default!;
    [Inject]
    public INaeTimeOrchestrator Orchestrator { get; set; } = default!;

    private readonly List<Lib.Models.OpenPracticeLaneConfiguration> _laneConfigurations = new();
    private readonly List<TrackDetails> _tracks = new();
    private readonly List<Pilot> _pilots = new();
    private readonly List<SessionDetails> _sessionDetails = new();
    private readonly CancellationTokenSource _source = new();

    private OpenPracticeSession? _selectedSession;
    private Guid? _activeSessionId;
    private bool _isLaneConfigCollapsed = false;
    private async Task When(OpenPracticeSessionActivated x)
    {
        _activeSessionId = x.SessionId;
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    private async Task When(SessionDeactivated x)
    {
        if (_activeSessionId != x.SessionId)
        {
            return;
        }

        _activeSessionId = null;
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    protected override async Task OnInitializedAsync()
    {
        _ = Receiver.Process<OpenPracticeSessionActivated>(_source.Token, When);
        _ = Receiver.Process<SessionDeactivated>(_source.Token, When);

        IEnumerable<Persistence.Abstractions.Management.Pilot>? pilotsResponse = await Persistence.Management.GetPilots();

        _pilots.AddRange(pilotsResponse.Select(x => new Pilot()
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            CallSign = x.CallSign
        }));

        IEnumerable<Persistence.Abstractions.Management.Track> tracks = await Persistence.Management.GetTracks();

        _tracks.AddRange(tracks.Select(x => new TrackDetails(x.Id, x.Name, x.MinimumLapTimeMilliseconds, x.MaximumLapTimeMilliseconds, x.Timers, x.AllowedLanes)));

        IEnumerable<Persistence.Abstractions.OpenPractice.OpenPracticeSession>? sessions = await Persistence.OpenPractice.GetOpenPracticeSessions();

        _sessionDetails.AddRange(sessions.Select(x => new SessionDetails
        {
            Id = x.Id,
            Name = x.Name,
            Type = SessionType.OpenPractice
        }));

        Persistence.Abstractions.Management.ActiveSession? activeSessionReponse = await Persistence.Management.GetActiveSession();

        if (activeSessionReponse != null)
        {
            _activeSessionId = activeSessionReponse.SessionId;
            await SetupForSession(activeSessionReponse.SessionId);
        }
    }
    private async Task SetupForSession(Guid sessionId)
    {
        _selectedSession = null;
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        Persistence.Abstractions.OpenPractice.OpenPracticeSession? practiceSession = await Persistence.OpenPractice.GetOpenPracticeSession(sessionId);
        if (practiceSession == null)
        {
            return;
        }

        Persistence.Abstractions.Management.Track? track = await Persistence.Management.GetTrack(practiceSession.TrackId);

        if (track == null)
        {
            return;
        }

        IEnumerable<LaneActiveTimings> timings = await Persistence.Timing.GetSessionActiveTimings(sessionId);

        byte allowedLanes = track.AllowedLanes;

        _selectedSession = new OpenPracticeSession
        {
            Id = practiceSession.Id,
            TrackId = practiceSession.TrackId,
            Name = practiceSession.Name,
            MinimumLapMilliseconds = practiceSession.MinimumLapMilliseconds,
            MaximumLapMilliseconds = practiceSession.MaximumLapMilliseconds,
            Lanes = GetLaneConfigurations(allowedLanes, practiceSession.ActiveLanes, timings).ToList(),
            Laps = practiceSession.Laps.Select(x => new OpenPracticeLap()
            {
                Id = x.Id,
                PilotId = x.PilotId,
                PilotName = _pilots.FirstOrDefault(y => y.Id == x.PilotId)?.CallSign,
                StartedUtc = x.StartedUtc,
                FinishedUtc = x.FinishedUtc,
                TotalMilliseconds = x.TotalMilliseconds,
                Status = x.Status switch
                {
                    NaeTime.Persistence.Abstractions.OpenPractice.LapStatus.Completed => OpenPracticeLapStatus.Completed,
                    NaeTime.Persistence.Abstractions.OpenPractice.LapStatus.Invalid => OpenPracticeLapStatus.Invalid,
                    _ => throw new NotImplementedException()
                }
            }).ToList(),
            TrackedConsecutiveLaps = practiceSession.TrackedConsecutiveLaps.ToList()
        };

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    private IEnumerable<Lib.Models.OpenPractice.OpenPracticeLaneConfiguration> GetLaneConfigurations(byte allowedLanes, IEnumerable<Persistence.Abstractions.OpenPractice.OpenPracticeLaneConfiguration> lanes, IEnumerable<LaneActiveTimings>? timings)
    {
        for (byte lane = 1; lane <= allowedLanes; lane++)
        {
            Lib.Models.OpenPracticeLaneConfiguration? laneConfig = _laneConfigurations.FirstOrDefault(x => x.Lane == lane);
            Persistence.Abstractions.OpenPractice.OpenPracticeLaneConfiguration? pilotLaneConfig = lanes.FirstOrDefault(x => x.Lane == lane);

            yield return new Lib.Models.OpenPractice.OpenPracticeLaneConfiguration()
            {
                LaneNumber = lane,
                BandId = laneConfig?.BandId,
                FrequencyInMhz = laneConfig?.FrequencyInMhz ?? 0,
                IsEnabled = laneConfig?.IsEnabled ?? false,
                PilotId = pilotLaneConfig?.PilotId,
                LastDetection = timings?.FirstOrDefault(x => x.Lane == lane)?.Lap?.StartedUtcTime
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
        await EventClient.PublishAsync(new OpenPracticeSessionConfigured(sessionId, sessionName, trackId, minimumLapMilliseconds, maximumLapMilliseconds));

        await Orchestrator.Management.ActivateOpenPracticeSession(sessionId);
        await Orchestrator.CommitAsync();

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
        IEnumerable<NaeTime.Persistence.Abstractions.Hardware.TimerDetails>? timers = await Persistence.Hardware.GetAllTimerDetails();

        if (timers == null)
        {
            return;
        }

        if (!timers.Any())
        {
            return;
        }

        Guid newTrackId = Guid.NewGuid();
        IEnumerable<NaeTime.Persistence.Abstractions.Hardware.TimerDetails> trackTimers = timers.Take(1);
        byte maxLanes = trackTimers.Max(x => x.MaxLanes);
        IEnumerable<Guid> timerIds = trackTimers.Select(x => x.Id);

        await Orchestrator.Management.CreateTrack($"Quick Track -{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", 0, null, timerIds);
        await Orchestrator.CommitAsync();

        Persistence.Abstractions.Management.Track? track = await Persistence.Management.GetTrack(newTrackId);

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

        return EventClient.PublishAsync(new OpenPracticeMinimumLapTimeConfigured(_selectedSession.Id, minimumLapMilliseconds));
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

        return EventClient.PublishAsync(new OpenPracticeMaximumLapTimeConfigured(_selectedSession.Id, maximumLapMilliseconds));
    }
    public async Task TrackConsecutiveLapsSelected(MenuEventArgs args)
    {
        if (args.Item == null)
        {
            return;
        }

        if (uint.TryParse(args.Item.Id, out uint lapCap))
        {
            await TrackConsecutiveLaps(lapCap);
        }
    }
    public Task TrackConsecutiveLaps(uint lapCap)
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
        return EventClient.PublishAsync(new ConsecutiveLapCountTracked(_selectedSession.Id, lapCap));
    }
    public void Dispose() => _source.Cancel();
}
