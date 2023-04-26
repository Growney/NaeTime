using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Store.Abstractions
{
    public interface IEventStorePersistentSubscriptionConnection
    {
        Task<bool> ConnectToPersistentSubscription(string stream, string groupName, int bufferSize, Func<ReadEventData, Task<ProjectionEventResult>> onEventAppeared);
    }
}
