using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions.Models
{
    public class NodeConfiguration
    {
        public NodeConfiguration(Guid nodeId, int? rssiTransmissionDelay, string? serverUri, int? rssiRetryCount, IEnumerable<RX5808ReceiverConfiguration> rX5808Receivers)
        {
            NodeId = nodeId;
            RssiTransmissionDelay = rssiTransmissionDelay;
            ServerUri = serverUri;
            RssiRetryCount = rssiRetryCount;
            RX5808Receivers = rX5808Receivers ?? throw new ArgumentNullException(nameof(rX5808Receivers));
        }

        public Guid NodeId { get; }
        public int? RssiTransmissionDelay { get; }
        public string? ServerUri { get; }
        public int? RssiRetryCount { get; }
        public IEnumerable<RX5808ReceiverConfiguration> RX5808Receivers { get; }

    }
}
