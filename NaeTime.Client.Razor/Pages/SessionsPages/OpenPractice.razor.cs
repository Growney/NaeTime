﻿using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Messages.Events.Activation;
using NaeTime.Messages.Events.Entities;
using NaeTime.Messages.Events.Hardware;
using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub.Abstractions;

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
        PublishSubscribe.Subscribe<LapInvalidated>(this, When);
        PublishSubscribe.Subscribe<OpenPracticeLapInvalidated>(this, When);
        PublishSubscribe.Subscribe<OpenPracticeLapRemoved>(this, When);
        PublishSubscribe.Subscribe<RssiLevelRecorded>(this, When);
        PublishSubscribe.Subscribe<LapStarted>(this, When);
        PublishSubscribe.Subscribe<OpenPracticeLapCompleted>(this, When);
        PublishSubscribe.Subscribe<OpenPracticeSingleLapLeaderboardPositionsChanged>(this, When);
        PublishSubscribe.Subscribe<OpenPracticeConsecutiveLapLeaderboardPositionsChanged>(this, When);
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
    public async Task When(LapStarted started)
    {
        if (_selectedSession == null)
        {
            return;
        }

        var sessionLane = _selectedSession.Lanes.FirstOrDefault(x => x.LaneNumber == started.Lane);
        if (sessionLane == null)
        {
            return;
        }

        sessionLane.LapStarted = started.StartedUtcTime;

        await InvokeAsync(StateHasChanged);
    }
    public async Task When(LapInvalidated invalidated)
    {
        if (_selectedSession == null)
        {
            return;
        }

        var sessionLane = _selectedSession.Lanes.FirstOrDefault(x => x.LaneNumber == invalidated.Lane);
        if (sessionLane == null)
        {
            return;
        }

        sessionLane.LapStarted = null;

        await InvokeAsync(StateHasChanged);
    }
    public async Task When(RssiLevelRecorded rssiLevelRecorded)
    {
        if (_selectedSession == null)
        {
            return;
        }

        var sessionLane = _selectedSession.Lanes.FirstOrDefault(x => x.LaneNumber == rssiLevelRecorded.Lane);
        if (sessionLane == null)
        {
            return;
        }

        sessionLane.RssiValue = rssiLevelRecorded.Level;
        if (sessionLane.MaxRssiValue < rssiLevelRecorded.Level)
        {
            sessionLane.MaxRssiValue = rssiLevelRecorded.Level;
        }
        await InvokeAsync(StateHasChanged);
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

        await InvokeAsync(StateHasChanged);
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
        await InvokeAsync(StateHasChanged);
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

        await InvokeAsync(StateHasChanged);
    }
    public async Task When(SessionActivated session)
    {
        _activeSessionId = session.SessionId;

        await InvokeAsync(StateHasChanged);
    }
    public async Task When(SessionDeactivated session)
    {
        _activeSessionId = null;

        await InvokeAsync(StateHasChanged);
    }
    public async Task When(OpenPracticeSingleLapLeaderboardPositionsChanged changes)
    {
        if (_selectedSession?.Id != changes.SessionId)
        {
            return;
        }

        var leaderboard = _selectedSession.SingleLapLeaderboards.FirstOrDefault(x => x.Id == changes.LeaderboardId);

        if (leaderboard == null)
        {
            return;
        }

        leaderboard.Positions = changes.NewPositions.Select(x => new SingleLapLeaderboardPosition()
        {
            Position = x.Position,
            PilotId = x.PilotId,
            LapId = x.LapId,
            LapMilliseconds = x.LapMilliseconds,
            CompletionUtc = x.CompletionUtc,
            PilotName = _pilots.FirstOrDefault(y => y.Id == x.PilotId)?.CallSign
        }).ToList();

        await InvokeAsync(StateHasChanged);
    }
    public async Task When(OpenPracticeConsecutiveLapLeaderboardPositionsChanged changes)
    {
        if (_selectedSession?.Id != changes.SessionId)
        {
            return;
        }

        var leaderboard = _selectedSession.ConsecutiveLapLeaderboards.FirstOrDefault(x => x.Id == changes.LeaderboardId);

        if (leaderboard == null)
        {
            return;
        }

        leaderboard.Positions = changes.NewPositions.Select(x => new ConsecutiveLapsLeaderboardPosition()
        {
            Position = x.Position,
            PilotId = x.PilotId,
            TotalLaps = x.TotalLaps,
            TotalMilliseconds = x.TotalMilliseconds,
            LastLapCompletionUtc = x.LastLapCompletion,
            IncludedLaps = x.IncludedLaps,
            PilotName = _pilots.FirstOrDefault(p => p.Id == x.PilotId)?.CallSign
        }).ToList();

        await InvokeAsync(StateHasChanged);
    }
    private async Task SetupForSession(Guid sessionId)
    {
        _selectedSession = null;

        var practiceSessionResponse = await PublishSubscribe.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(sessionId));
        if (practiceSessionResponse == null)
        {
            return;
        }

        var track = await PublishSubscribe.Request<TrackRequest, TrackResponse>(new TrackRequest(practiceSessionResponse.TrackId));

        if (track == null)
        {
            return;
        }


        var timings = await PublishSubscribe.Request<ActiveTimingsRequest, ActiveTimingsResponse>(new ActiveTimingsRequest(sessionId));


        var allowedLanes = track.AllowedLanes;

        _selectedSession = new OpenPracticeSession
        {
            Id = practiceSessionResponse.SessionId,
            TrackId = practiceSessionResponse.TrackId,
            Name = practiceSessionResponse.Name,
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
            SingleLapLeaderboards = practiceSessionResponse.SingleLapLeaderboards.Select(x =>
             new SingleLapLeaderboard()
             {
                 Id = x.LeaderboardId,
                 Positions = x.Positions.Select(y => new SingleLapLeaderboardPosition()
                 {
                     Position = y.Position,
                     PilotId = y.PilotId,
                     LapId = y.LapId,
                     LapMilliseconds = y.TotalMilliseconds,
                     CompletionUtc = y.CompletionUtc,
                     PilotName = _pilots.FirstOrDefault(x => x.Id == y.PilotId)?.CallSign
                 }).ToList()
             }).ToList(),
            ConsecutiveLapLeaderboards = practiceSessionResponse.ConsecutiveLapLeaderboards.Select(x =>
            new ConsecutiveLapsLeaderboard()
            {
                Id = x.LeaderboardId,
                ConsecutiveLaps = x.ConsecutiveLaps,
                Positions = x.Positions.Select(y => new ConsecutiveLapsLeaderboardPosition()
                {
                    Position = y.Position,
                    PilotId = y.PilotId,
                    TotalLaps = y.TotalLaps,
                    TotalMilliseconds = y.TotalMilliseconds,
                    LastLapCompletionUtc = y.LastLapCompletionUtc,
                    IncludedLaps = y.IncludedLaps,
                    PilotName = _pilots.FirstOrDefault(x => x.Id == y.PilotId)?.CallSign
                }).ToList()
            }).ToList()
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
        await PublishSubscribe.Dispatch(new OpenPracticeSessionConfigured(sessionId, DateTime.Now.ToShortDateString(), trackId, minimumLapMilliseconds, maximumLapMilliseconds));

        await PublishSubscribe.Dispatch(new SessionActivated(sessionId, SessionActivated.SessionType.OpenPractice));

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

        await PublishSubscribe.Dispatch(new TrackCreated(newTrackId, $"Quick Track -{DateTime.Now.ToShortDateString()}", 0, null, timersResponse.Timers.Take(1).Select(x => x.Id)));

        var sessionId = Guid.NewGuid();

        await StartNewSessionOnTrack(sessionId, newTrackId, 0, null);
    }
    public async Task AddSingleLapLeaderboard()
    {
        if (_selectedSession == null)
        {
            return;
        }
        var leaderboardId = Guid.NewGuid();
        _selectedSession.SingleLapLeaderboards.Add(new SingleLapLeaderboard()
        {
            Id = leaderboardId,
        });
        await PublishSubscribe.Dispatch(new OpenPracticeSingleLapLeaderboardConfigured(_selectedSession.Id, leaderboardId));



    }
    public async Task AddConsecutiveLapsLeaderboard(uint lapCap)
    {
        if (_selectedSession == null)
        {
            return;
        }
        var leaderboardId = Guid.NewGuid();
        _selectedSession.ConsecutiveLapLeaderboards.Add(new ConsecutiveLapsLeaderboard()
        {
            Id = leaderboardId,
            ConsecutiveLaps = lapCap,
        });
        await PublishSubscribe.Dispatch(new OpenPracticeConsecutiveLapLeaderboardConfigured(_selectedSession!.Id, leaderboardId, lapCap));
    }

    public async Task OnLaneFrequyencyChanged(byte lane, byte? bandId, int frequencyInMhz)
    {
        if (_selectedSession == null)
        {
            return;
        }

        var laneConfig = _selectedSession.Lanes.FirstOrDefault(x => x.LaneNumber == lane);

        if (laneConfig == null)
        {
            return;
        }

        if (laneConfig.BandId == bandId && laneConfig.FrequencyInMhz == frequencyInMhz)
        {
            return;
        }

        laneConfig.BandId = bandId;
        laneConfig.FrequencyInMhz = frequencyInMhz;

        await PublishSubscribe.Dispatch(new LaneRadioFrequencyConfigured(lane, bandId, frequencyInMhz));
    }
    public async Task OnLaneEnabledChanged(byte lane, bool isEnabled)
    {
        if (_selectedSession == null)
        {
            return;
        }

        var laneConfig = _selectedSession.Lanes.FirstOrDefault(x => x.LaneNumber == lane);

        if (laneConfig == null)
        {
            return;
        }

        if (laneConfig.IsEnabled == isEnabled)
        {
            return;
        }

        laneConfig.IsEnabled = isEnabled;

        if (isEnabled)
        {
            await PublishSubscribe.Dispatch(new LaneEnabled(lane));
        }
        else
        {
            await PublishSubscribe.Dispatch(new LaneDisabled(lane));
        }
    }
    public async Task OnLanePilotChanged(byte lane, Guid? pilotId)
    {
        if (_selectedSession == null)
        {
            return;
        }
        if (pilotId == null)
        {
            throw new NotImplementedException();
        }
        var laneConfig = _selectedSession.Lanes.FirstOrDefault(x => x.LaneNumber == lane);

        if (laneConfig == null)
        {
            return;
        }

        if (laneConfig.PilotId == pilotId)
        {
            return;
        }

        laneConfig.PilotId = pilotId;

        await PublishSubscribe.Dispatch(new OpenPracticeLanePilotSet(_selectedSession.Id, pilotId.Value, lane));
    }
    public Task OnLaneDetectionTriggered(byte lane, byte split)
    {
        if (_selectedSession == null)
        {
            return Task.CompletedTask;
        }
        return PublishSubscribe.Dispatch(new SessionDetectionTriggered(_selectedSession.Id, lane, split));

    }
    public Task OnLaneInvalidateTriggered(byte lane, byte split)
    {
        if (_selectedSession == null)
        {
            return Task.CompletedTask;
        }
        return PublishSubscribe.Dispatch(new SessionInvalidationTriggered(_selectedSession.Id, lane));

    }
    public void Dispose()
    {
        PublishSubscribe.Unsubscribe(this);
    }
}
