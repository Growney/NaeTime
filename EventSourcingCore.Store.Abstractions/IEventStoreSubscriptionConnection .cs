using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Store.Abstractions
{
    public interface IEventStoreSubscriptionConnection
    {
        Task<bool> SubscribeToAllAsync(Func<ReadEventData, Task> onEventAppeared, Func<Task<StorePosition?>> position = null);
        Task<bool> SubscribeToStreamAsync(string name, Func<ReadEventData, Task> onEventAppeared, Func<Task<StreamPosition?>> position = null);
    }
}
