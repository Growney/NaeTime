using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Store.Abstractions
{
    public interface IEventStoreStreamConnection
    {

        IAsyncEnumerable<ReadEventData> ReadStreamEvents(string streamname, StreamDirection direction, StreamPosition version);

        Task AppendToStreamAsync(string streamName, IEnumerable<EventData> events, StreamPosition expectedVersion);
        Task AppendToStreamAsync(string streamName, IEnumerable<EventData> events, StreamState expectedState);

    }
}