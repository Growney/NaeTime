using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class RX5808Dto
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string? Identifier { get; set; }
        public bool UseAnalogToDigitalConverter { get; set; }
        public byte RSSIPin { get; set; }
        public byte DataPin { get; set; }
        public byte SelectPin { get; set; }
        public byte ClockPin { get; set; }
    }
}
