using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions.Models;
[DebuggerDisplay("{Type}")]
public record TimingMoment(Guid SessionId, Guid TrackId, Guid PilotId,Guid? MomentId,TimingMoment.TimingMomentType Type)
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
