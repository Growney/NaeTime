using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class FlightDto
    {
        public Guid Id { get; set; }
        public int Frequency { get; set; }
        public Guid TrackId { get; set; }
        public Guid PilotId { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public List<LapDto> Laps { get; set; } = new();
        public List<SplitDto> Splits { get; set; } = new();
        public List<RssiStreamDto> RssiStreams { get; set; } = new();
    }
}
