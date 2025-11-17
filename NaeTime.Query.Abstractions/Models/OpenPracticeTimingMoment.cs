using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions.Models;
[DebuggerDisplay("{Type}")]
public record OpenPracticeTimingMoment(Guid SessionId, Guid TrackId, Guid PilotId,Guid? MomentId,OpenPracticeTimingMoment.TimingMomentType Type)
{
    public enum TimingMomentType
    {
        LapStarted,
        SplitStarted,
        SplitCompleted,
        SplitSkipped,
        DetectionDiscardedDueToMinimiumLapTime,
        LapCompleted,
        LapInvalidatedDueToMaximumLapTime,
        DetectionDiscardedAsItsInvalid,
        LapStoppedByEndOfPack,
        EndOfPack,
    }
}
