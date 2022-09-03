using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface ICommunicationManager
    {
        void Configure(Guid nodeId, string serviceUri, int? transmissionDelay, int? rssiRetryCount);
    }
}
