using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Events;
public record PilotOpenPracticeSessionTimingStarted(Guid PilotId, Guid SessionId);
public record OpenPracticeDetectionAddedToPilot(Guid PilotId, Guid SessionId, Guid DetectionId, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeDetectionRemovedFromPilot(Guid PilotId, Guid SessionId, Guid DetectionId);
public record OpenPracticeDetectionValidated(Guid PilotId, Guid SessionId, Guid DetectionId);
public record OpenPracticeDetectionInvalidated(Guid PilotId, Guid SessionId, Guid DetectionId);
public record OpenPracticeLapStarted(Guid PilotId, Guid SessionId, Guid LapId, Guid StartDetectionId);
public record OpenPracticeLapCompleted(Guid PilotId, Guid SessionId, Guid LapId, Guid StartDetectionId, Guid EndDetectionId,long DurationMilliseconds);
public record OpenPracticeLapTimedOut(Guid PilotId, Guid SessionId, Guid LapId);
public record OpenPracticeLapIncluded(Guid PilotId, Guid SessionId, Guid LapId);
public record OpenPracticeLapDurationIncreased(Guid PilotId, Guid SessionId, Guid LapId, long PreviousDurationMilliseconds, long NewDurationMilliseconds);
public record OpenPracticeLapDurationDecreased(Guid PilotId, Guid SessionId, Guid LapId, long PreviousDurationMilliseconds, long NewDurationMilliseconds);
public record OpenPracticeLapStartDetectionChanged(Guid PilotId, Guid SessionId, Guid LapId, Guid PreviousStartDetectionId, Guid NewStartDetectionId);
public record OpenPracticeLapEndDetectionChanged(Guid PilotId, Guid SessionId, Guid LapId, Guid PreviousEndDetectionId, Guid NewEndDetectionId);

