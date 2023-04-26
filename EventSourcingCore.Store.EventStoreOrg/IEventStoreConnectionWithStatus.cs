using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public interface IEventStoreConnectionWithStatus : IEventStoreConnection
    {
        bool IsConnected { get; }
        string ConnectionStatus { get; }
        Task<bool> ConnectWithStatusAsync();
    }
}
