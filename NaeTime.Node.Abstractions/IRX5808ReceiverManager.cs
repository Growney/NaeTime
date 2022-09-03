using NaeTime.Node.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface IRX5808ReceiverManager
    {
        Task SetupReceivers(IEnumerable<RX5808ReceiverConfiguration> receivers);
    }
}
