using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class NodeDto
    {
        public string? Address { get; set; }
        public Guid? Id { get; set; }
        public string? Identifier { get; set; }
        public int? RssiTransmissionDelay { get; set; }
        public int? RssiRetryCount { get; set; }
        public IEnumerable<RX5808Dto>? RX5808Receivers { get; set; }

    }
}
