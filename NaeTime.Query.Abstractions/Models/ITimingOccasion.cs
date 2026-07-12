using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Query.Abstractions.Models;

public interface ITimingOccasion
{
    public long SoftwareTime { get; }
    public DateTime UtcTime { get; }
}
