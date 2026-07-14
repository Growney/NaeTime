using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Events.Integration;

public record SessionTimingChanged(Guid SessionId, Guid TriggerDetection);

