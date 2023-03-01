using NaeTime.Shared;
using NaeTime.Shared.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Client.Abstractions
{
    public interface INodeClient : IDisposable
    {
        Task ConfigureAsync(ConfigurationDto dto);
        Task<ConfigurationDto?> GetConfigurationAsync();
        Task<RssiStreamDto?> GetRssiStreamAsync(Guid streamId);
        Task<RssiStreamDto?> StartRssiStreamAsync(Guid streamId, int frequency, RssiReceiverTypeDto? receiverType);
        Task StopRssiStreamAsync(Guid streamId);
        Task<List<RssiReceiverDto>> GetReceiversAsync();
    }
}
