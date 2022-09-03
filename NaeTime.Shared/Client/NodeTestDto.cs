using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class NodeTestDto
    {
        public Guid NodeId { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsSetup { get; set; }
        public bool AreAllRX5808Setup { get; set; }
        public IEnumerable<RX5808ReceiverTestDto>? RX5808Tests { get; set; }
    }
}
