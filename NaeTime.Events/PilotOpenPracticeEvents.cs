using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Events;
public record PilotOpenPracticeSessionTimingStarted(Guid PilotId, Guid SessionId);
public record OpenPracticeDetectionAssignedToPilot(Guid DetectionId, Guid PilotId, Guid SessionId, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);

