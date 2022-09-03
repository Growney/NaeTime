using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Node
{
    public class ConfigurationDto
    {
        public Guid Id { get; set; }
        public int? RssiTransmissionDelay { get; set; }
        public string? ServerUri { get; set; }
        public int? RssiRetryCount { get; set; }
        public List<RX5808Dto>? RX5808Receivers { get; set; }

    }
}
