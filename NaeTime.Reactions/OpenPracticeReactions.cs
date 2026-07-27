using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace NaeTime.Reactions;

internal class OpenPracticeReactions
{
    private readonly IImmersionRCLapRFCommandHandler _immersionRCLapRFCommandHandler;
    private readonly INaeTimeNodeCommandHandler _naeTimeNodeCommandHandler;
    private readonly IHardwareQueryHandler _hardwareQueryHandler;
    private readonly ITimingQueryHandler _openPracticeQueryHandler;
    private readonly ITrackQueryHandler _trackQueryHandler;
    private readonly ISessionQueryHandler _sessionQueryHandler;
    private readonly IAggregateRepository _aggregateRepository;

    public OpenPracticeReactions(IAggregateRepository aggregateRepository, IImmersionRCLapRFCommandHandler immersionRCLapRFCommandHandler, INaeTimeNodeCommandHandler naeTimeNodeCommandHandler, IHardwareQueryHandler hardwareQueryHandler, ITimingQueryHandler openPracticeQueryHandler, ITrackQueryHandler trackQueryHandler, ISessionQueryHandler sessionQueryHandler)
    {
        _immersionRCLapRFCommandHandler = immersionRCLapRFCommandHandler;
        _naeTimeNodeCommandHandler = naeTimeNodeCommandHandler;
        _hardwareQueryHandler = hardwareQueryHandler;
        _openPracticeQueryHandler = openPracticeQueryHandler;
        _trackQueryHandler = trackQueryHandler;
        _sessionQueryHandler = sessionQueryHandler;
        _aggregateRepository = aggregateRepository;
    }

    private async Task When(OpenPracticeSessionScheduled schedule)
    {
        NaeTime.Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(schedule.TrackId);

        if (track == null)
        {
            return;
        }
        
        for (byte currentLane = 0; currentLane < track.MaxLanes; currentLane++)
        {
            OpenPracticeSessionLane lane = _aggregateRepository.CreateNew<OpenPracticeSessionLane>(() => new OpenPracticeSessionLane(schedule.SessionId, currentLane, Hardware.Frequency.Band.R.Id, Hardware.Frequency.Band.R.Frequencies.ElementAt(currentLane).FrequencyInMhz));
            await _aggregateRepository.Save<OpenPracticeSessionLane,OpenPracticeSessionLane.LaneKey>(lane);
        }
    }

    private async Task When(OpenPracticeSessionTrackChanged changed)
    {
        NaeTime.Query.Abstractions.Models.Track? newTrack = await _trackQueryHandler.GetTrack(changed.NewTrackId);

        if (newTrack == null)
        {
            throw new Exception("Invalid new track for change");
        }

        NaeTime.Query.Abstractions.Models.Track? oldTrack = await _trackQueryHandler.GetTrack(changed.OldTrackId);

        if (oldTrack == null)
        {
            throw new Exception("Invalid old track for change");
        }

        if(newTrack.MaxLanes == oldTrack.MaxLanes)
        {
            return;
        }
        else if(oldTrack.MaxLanes > newTrack.MaxLanes)
        {
            // Close unused lanes
            for (byte lane = newTrack.MaxLanes; lane < oldTrack.MaxLanes; lane++)
            {
                OpenPracticeSessionLane? sessionLane = await _aggregateRepository.Get<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(new OpenPracticeSessionLane.LaneKey(changed.SessionId, lane));
                sessionLane?.DisableLane();
                if(sessionLane != null)
                {
                    await _aggregateRepository.Save<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(sessionLane);
                }
            }
        }
        else
        {
            for (byte lane = oldTrack.MaxLanes; lane < newTrack.MaxLanes; lane++)
            {
                OpenPracticeSessionLane? sessionLane = await _aggregateRepository.Get<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(new OpenPracticeSessionLane.LaneKey(changed.SessionId, lane))
                    ?? _aggregateRepository.CreateNew<OpenPracticeSessionLane>(() => new OpenPracticeSessionLane(changed.SessionId, lane, Hardware.Frequency.Band.R.Id, Hardware.Frequency.Band.R.Frequencies.ElementAt(lane).FrequencyInMhz));
                await _aggregateRepository.Save<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(sessionLane);
            }
        }
    }

    private async Task When(OpenPracticeSessionActivated activated)
    {
        NaeTime.Query.Abstractions.Models.OpenPracticeSession? session = await _openPracticeQueryHandler.GetByIdAsync(activated.SessionId);

        if (session is null)
        {
            return;
        }

        NaeTime.Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(session.TrackId);

        if (track is null)
        {
            return;
        }

        foreach (NaeTime.Query.Abstractions.Models.Detector detector in track.Detectors)
        {
            if (detector.Type.HasFlag(NaeTime.Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                foreach (NaeTime.Query.Abstractions.Models.OpenPracticeLane lane in session.Lanes)
                {
                    await _immersionRCLapRFCommandHandler.SetupLaneForSession(detector.Id, lane.Lane, lane.IsEnabled, lane.BandId, lane.FrequencyInMHz);
                }
            }
            if (detector.Type.HasFlag(NaeTime.Query.Abstractions.Models.DetectorType.NaeTime))
            {
                foreach (NaeTime.Query.Abstractions.Models.OpenPracticeLane lane in session.Lanes)
                {
                    await _naeTimeNodeCommandHandler.SetupLaneForSession(detector.Id, lane.Lane, lane.IsEnabled, lane.BandId, lane.FrequencyInMHz);
                }
            }
        }
    }

    private async Task When(OpenPracticeSessionCloned cloned)
    {
        var oldSession = await _openPracticeQueryHandler.GetByIdAsync(cloned.SessionId);

        if(oldSession == null)
        {
            return;
        }

        var newSession = _aggregateRepository.CreateNew<Command.Aggregates.OpenPracticeSession>(() => new Command.Aggregates.OpenPracticeSession(cloned.NewSessionId, oldSession.TrackId, oldSession.Name + " - clone", oldSession.MinimumLapTime, oldSession.MaximumLapTime));

        await _aggregateRepository.Save<Command.Aggregates.OpenPracticeSession>(newSession);
    }

    private async Task When(OpenPracticeSessionLanePilotSet pilot)
    {
        OpenPracticePilotTiming pilotTiming = await _aggregateRepository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(new OpenPracticePilotTiming.PilotKey(pilot.SessionId, pilot.PilotId))
            ?? _aggregateRepository.CreateNew<OpenPracticePilotTiming>(() => new OpenPracticePilotTiming(pilot.SessionId, pilot.PilotId));

        await _aggregateRepository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(pilotTiming);
    }

    private async Task When(OpenPracticeSessionLaneVideoFrequencyTuned tuned)
    {
        NaeTime.Query.Abstractions.Models.Session? activeSession = await _sessionQueryHandler.GetActiveSession();
        if (activeSession is null || activeSession.Id != tuned.SessionId)
        {
            return;
        }

        NaeTime.Query.Abstractions.Models.OpenPracticeSession? session = await _openPracticeQueryHandler.GetByIdAsync(tuned.SessionId);

        if (session is null)
        {
            return;
        }

        NaeTime.Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(session.TrackId);

        if (track is null)
        {
            return;
        }

        foreach (Query.Abstractions.Models.Detector detector in track.Detectors)
        {
            if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneFrequency(detector.Id, tuned.Lane, tuned.BandId, tuned.FrequencyInMHz);
            }
            else if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.NaeTime))
            {
                await _naeTimeNodeCommandHandler.RequestLaneFrequency(detector.Id, tuned.Lane, tuned.BandId, tuned.FrequencyInMHz);
            }
        }
    }
    private async Task When(OpenPracticeSessionLaneStatusSet status)
    {
        NaeTime.Query.Abstractions.Models.Session? activeSession = await _sessionQueryHandler.GetActiveSession();
        if (activeSession is null || activeSession.Id != status.SessionId)
        {
            return;
        }

        NaeTime.Query.Abstractions.Models.OpenPracticeSession? session = await _openPracticeQueryHandler.GetByIdAsync(status.SessionId);

        if (session is null)
        {
            return;
        }

        NaeTime.Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(session.TrackId);

        if (track is null)
        {
            return;
        }

        foreach (Query.Abstractions.Models.Detector detector in track.Detectors)
        {
            if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneStatus(detector.Id, status.Lane, status.IsEnabled);
            }
            else if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.NaeTime))
            {
                await _naeTimeNodeCommandHandler.RequestLaneStatus(detector.Id, status.Lane, status.IsEnabled);
            }
        }
    }
}
