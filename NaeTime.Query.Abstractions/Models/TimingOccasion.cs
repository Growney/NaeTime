using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Query.Abstractions.Models;

public record OpenPracticePilotPackEnd(Guid Id, Guid SessionId, Guid PilotId, long SoftwareTime, DateTime UtcTime) : ITimingOccasion;