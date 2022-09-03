using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Node
{
    public class RssiStreamDto
    {
        public Guid Id { get; set; }
        public Guid ReceiverId { get; set; }
        public RssiReceiverTypeDto ReceiverType { get; set; }
        public long StartTick { get; set; }
        public int TunedFrequency { get; set; }
        public int AssignedFrequency { get; set; }
    }
}
