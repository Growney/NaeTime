using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class LapDto
    {
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public long StartTick { get; set; }
        public long? EndTick { get; set; }
    }
}
