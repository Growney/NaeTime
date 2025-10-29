using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Query.Abstractions;

namespace NaeTime.Reactions;
internal class OpenPracticeReactions
{
    private readonly IImmersionRCLapRFCommandHandler _immersionRCLapRFCommandHandler;
    private readonly IHardwareQueryHandler _hardwareQueryHandler;
    private readonly IOpenPracticeQueryHandler _openPracticeQueryHandler;
    private readonly ITrackQueryHandler _trackQueryHandler;
    private readonly ISessionQueryHandler _sessionQueryHandler;

    public OpenPracticeReactions(IImmersionRCLapRFCommandHandler immersionRCLapRFCommandHandler, IHardwareQueryHandler hardwareQueryHandler, IOpenPracticeQueryHandler openPracticeQueryHandler, ITrackQueryHandler trackQueryHandler, ISessionQueryHandler sessionQueryHandler)
    {
        _immersionRCLapRFCommandHandler = immersionRCLapRFCommandHandler;
        _hardwareQueryHandler = hardwareQueryHandler;
        _openPracticeQueryHandler = openPracticeQueryHandler;
        _trackQueryHandler = trackQueryHandler;
        _sessionQueryHandler = sessionQueryHandler;
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
        }
    }

    private async Task When(OpenPracticeSessionLaneVideoFrequencyTuned tuned)
    {
        NaeTime.Query.Abstractions.Models.Session? activeSession = await _sessionQueryHandler.GetActiveSession();
        if (activeSession is null || activeSession.Id != tuned.SessionId)
        {
            return;
        }

        IEnumerable<Query.Abstractions.Models.Detector> detectors = await _hardwareQueryHandler.GetDetectors(tuned.TimerIds);
        foreach (Query.Abstractions.Models.Detector detector in detectors)
        {
            if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneFrequency(detector.Id, tuned.Lane, tuned.BandId, tuned.FrequencyInMHz);
            }
        }
    }

    private async Task When(OpenPracticeSessionLaneEnabled enabled)
    {

        NaeTime.Query.Abstractions.Models.Session? activeSession = await _sessionQueryHandler.GetActiveSession();
        if (activeSession is null || activeSession.Id != enabled.SessionId)
        {
            return;
        }

        IEnumerable<Query.Abstractions.Models.Detector> detectors = await _hardwareQueryHandler.GetDetectors(enabled.TimerIds);
        foreach (Query.Abstractions.Models.Detector detector in detectors)
        {
            if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneStatus(detector.Id, enabled.Lane, true);
            }
        }
    }
    private async Task When(OpenPracticeSessionLaneDisabled disabled)
    {
        NaeTime.Query.Abstractions.Models.Session? activeSession = await _sessionQueryHandler.GetActiveSession();
        if (activeSession is null || activeSession.Id != disabled.SessionId)
        {
            return;
        }

        IEnumerable<Query.Abstractions.Models.Detector> detectors = await _hardwareQueryHandler.GetDetectors(disabled.TimerIds);
        foreach (Query.Abstractions.Models.Detector detector in detectors)
        {
            if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneStatus(detector.Id, disabled.Lane, false);
            }
        }
    }
}
