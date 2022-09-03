using NaeTime.Node.Abstractions.Enumeration;
using NaeTime.Node.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface IRssiReceiverManager
    {
        event EventHandler OnReceiversChanged;
        RssiReceiverType ReceiverType { get; }

        bool CanHandleNewStream(int frequency);
        bool IsStreamManaged(Guid streamId);
        Task<RssiStream?> EnableStreamAsync(Guid streamId, int frequency);
        void DisableStream(Guid streamId);
        RssiStream? GetStream(Guid streamId);

        IEnumerable<IRssiReceiver> GetReceivers();
        IEnumerable<IRssiReceiver> GetEnabledReceivers();
    }
}
