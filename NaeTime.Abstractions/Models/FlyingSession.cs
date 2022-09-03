using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Models
{
    public class FlyingSession
    {
        public Guid Id { get; set; }
        public Guid HostPilotId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public Guid TrackId { get; set; }
        public List<Flight> Flights { get; set; } = new();

        public bool IsPublic { get; set; }
        public List<AllowedPilot> AllowedPilots { get; set; } = new();
    }
}
