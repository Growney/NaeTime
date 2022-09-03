using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Node
{
    public class RssiReceiverDto
    {
        public Guid Id { get; set; }
        public Guid StreamId { get; set; }
        public int AssignedFrequency { get; set; }
        public int TunedFrequency { get; set; }
        public bool IsRssiStreamEnabled { get; set; }
    }
}
