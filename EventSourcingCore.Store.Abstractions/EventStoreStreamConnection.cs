using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Store.Abstractions
{
    public class EventStoreStreamConnection : IEventStreamConnection
    {
        private readonly IEventStoreStreamConnection _connection;

        public EventStoreStreamConnection(IEventStoreStreamConnection connection)
        {
            _connection = connection;
        }
        public Task AppendToStreamAsync(string streamName, IEnumerable<ReadEventData> events, StreamState expectedState)
        {
            return _connection.AppendToStreamAsync(streamName, events, expectedState);
        }
        public Task AppendToStreamAsync(string streamName, ReadEventData e, StreamState expectedState)
        {
            return AppendToStreamAsync(streamName, new ReadEventData[] { e }, expectedState);
        }
        public Task AppendToStreamAsync(string streamName, IEnumerable<ReadEventData> events, StreamPosition expectedVersion)
        {
            return _connection.AppendToStreamAsync(streamName, events, expectedVersion);
        }
        public Task AppendToStreamAsync(string streamName, ReadEventData e, StreamPosition expectedVersion)
        {
            return AppendToStreamAsync(streamName, new ReadEventData[] { e }, expectedVersion);
        }
    }
}
