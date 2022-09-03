using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class RssiStreamDto
    {
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public Guid NodeId { get; set; }
        public Guid ReceiverId { get; set; }
        public RssiReceiverTypeDto ReceiverType { get; }
        public long StartTick { get; }
        public int TunedFrequency { get; set; }
        public RssiBoundaryDto? Boundary { get; set; }
        public List<RssiStreamPassDto> Passes { get; set; } = new();
        public List<RssiStreamReadingDto> Readings { get; set; } = new();
    }
}
