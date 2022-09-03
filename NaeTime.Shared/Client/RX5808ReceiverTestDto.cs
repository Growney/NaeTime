using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class RX5808ReceiverTestDto
    {
        public Guid Id { get; set; }
        public bool IsConfigured { get; set; }
        public int TunedFrequency { get; set; }
        public int AssignedFrequency { get; set; }
    }
}
