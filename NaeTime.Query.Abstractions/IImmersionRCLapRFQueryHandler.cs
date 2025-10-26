using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions;
public interface IImmersionRCLapRFQueryHandler
{
    public Task<IEnumerable<Models.ImmersionRCLapRFLane>> GetDesiredLaneConfigurations(Guid timerId);
}
