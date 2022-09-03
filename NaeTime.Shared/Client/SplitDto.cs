using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class SplitDto
    {
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public int Position { get; set; }
        public Guid StartGate { get; set; }
        public long StartTick { get; set; }
        public Guid EndGate { get; set; }
        public long? EndTick { get; set; }

    }
}
