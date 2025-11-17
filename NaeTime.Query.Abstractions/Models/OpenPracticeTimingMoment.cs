using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeTimingMoment(Guid SessionId, Guid TrackId, Guid PilotId,OpenPracticeTimingMoment.TimingMomentType Type, OpenPracticeDetection Detection)
{
    public enum TimingMomentType
    {
        DetectionDiscardedDueToMinimumLapTime,
        LapStarted,
        LapCompleted,
        LapDiscardedDueToMaximumLapTime,
        DetectionDiscardedDueToInvalidDetection
    }
}
