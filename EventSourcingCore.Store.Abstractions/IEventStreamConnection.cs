using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingCore.Store.Abstractions
{
    public interface IEventStreamConnection
    {
        Task AppendToStreamAsync(string streamName, IEnumerable<ReadEventData> events, StreamState expectedState);
        Task AppendToStreamAsync(string streamName, ReadEventData e, StreamState expectedState);
        public Task AppendToStreamAsync(string streamName, IEnumerable<ReadEventData> events, StreamPosition expectedVersion);
        public Task AppendToStreamAsync(string streamName, ReadEventData e, StreamPosition expectedVersion);
    }
}
