using NaeTime.Node.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface INodeConfigurationManager
    {
        Task ApplyConfigurationAsync(NodeConfiguration configuration);
        Task StoreConfigurationAsync(NodeConfiguration configuration);
        Task<NodeConfiguration?> GetConfigurationAsync();
    }
}
