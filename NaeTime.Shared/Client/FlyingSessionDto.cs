using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class FlyingSessionDto
    {
        public Guid Id { get; set; }
        public Guid HostPilotId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid TrackId { get; set; }
        public List<FlightDto> Flights { get; set; } = new();

        public bool IsPublic { get; set; }
        public List<PilotDto> AllowedPilots { get; set; } = new();

        public List<LapDto> Laps { get; set; } = new();
    }
}
