using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Query.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Reactions;
internal class OpenPracticeReactions
{
    private readonly IImmersionRCLapRFCommandHandler _immersionRCLapRFCommandHandler;
    private readonly IHardwareQueryHandler _hardwareQueryHandler;

    public OpenPracticeReactions(IImmersionRCLapRFCommandHandler immersionRCLapRFCommandHandler, IHardwareQueryHandler hardwareQueryHandler)
    {
        _immersionRCLapRFCommandHandler = immersionRCLapRFCommandHandler;
        _hardwareQueryHandler = hardwareQueryHandler;
    }

    private async Task When(OpenPracticeSessionLaneVideoFrequencyTuned tuned)
    {
        IEnumerable<Query.Abstractions.Models.Detector> detectors = await _hardwareQueryHandler.GetDetectors(tuned.TimerIds);
        foreach(Query.Abstractions.Models.Detector detector in detectors)
        {
            if(detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneFrequency(detector.Id,tuned.Lane,tuned.BandId,tuned.FrequencyInMHz);
            }
        }
    }

    private async Task When(OpenPracticeSessionLaneEnabled enabled)
    {
        IEnumerable<Query.Abstractions.Models.Detector> detectors = await _hardwareQueryHandler.GetDetectors(enabled.TimerIds);
        foreach (Query.Abstractions.Models.Detector detector in detectors)
        {
            if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneStatus(detector.Id, enabled.Lane,true);
            }
        }
    }
    private async Task When(OpenPracticeSessionLaneDisabled disabled)
    {
        IEnumerable<Query.Abstractions.Models.Detector> detectors = await _hardwareQueryHandler.GetDetectors(disabled.TimerIds);
        foreach (Query.Abstractions.Models.Detector detector in detectors)
        {
            if (detector.Type.HasFlag(Query.Abstractions.Models.DetectorType.ImmersionRC))
            {
                await _immersionRCLapRFCommandHandler.RequestLaneStatus(detector.Id, disabled.Lane, true);
            }
        }
    }
}
