using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Projections.Abstractions;
public interface ITimerConfigurationProjection
{
    public IEnumerable<NaeTime.Query.Abstractions.Models.ImmersionRCLapRFLane> GetImmersionRCLapRFLanesConfiguration(Guid timerId);
}
