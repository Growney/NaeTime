using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Client.Abstractions
{
    public interface INodeClientFactory
    {
        INodeClient CreateClient(string nodeAddress);
    }
}
