using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Query.Abstractions.Projections;

public interface ILaneConfigurationProjection
{
    public Guid? GetLanePilot(byte laneId);
}
