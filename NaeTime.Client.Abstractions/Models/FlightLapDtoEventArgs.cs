using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Abstractions.Models
{
    public class FlightLapDtoEventArgs
    {
        public FlightLapDtoEventArgs(LapDto lap)
        {
            Lap = lap ?? throw new ArgumentNullException(nameof(lap));
        }

        public LapDto Lap { get; }
    }
}
