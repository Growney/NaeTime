using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Store.Abstractions;
using EventData = EventStore.ClientAPI.EventData;
using ExpectedVersion = EventStore.ClientAPI.ExpectedVersion;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public class EventStoreOrgStreamConnection : IEventStoreStreamConnection
    {
        private readonly IEventStoreConnectionWithStatus _connection;

        public EventStoreOrgStreamConnection(IEventStoreConnectionWithStatus connection)
        {
            _connection = connection;

        }

        private List<EventData> GetData(IEnumerable<Event.Abstractions.EventData> events)
        {
            var eventStoreData = new List<EventData>();

            if (events != null)
            {
                foreach (var standardEvent in events)
                {
                    eventStoreData.Add(new EventData(standardEvent.ID, standardEvent.EventType, standardEvent.ContentType == "application/json", standardEvent.Data.ToArray(), standardEvent.Metadata.ToArray()));
                }
            }
            return eventStoreData;
        }
        public Task AppendToStreamAsync(string streamName, IEnumerable<Event.Abstractions.EventData> events, Abstractions.StreamState expectedState)
        {
            long version;
            if (expectedState == Abstractions.StreamState.StreamExists)
            {
                version = ExpectedVersion.StreamExists;
            }
            else if (expectedState == Abstractions.StreamState.NoStream)
            {
                version = ExpectedVersion.NoStream;
            }
            else
            {
                version = ExpectedVersion.Any;
            }

            return AppendToStreamAsync(streamName, events, version);
        }
        public Task AppendToStreamAsync(string streamName, IEnumerable<Event.Abstractions.EventData> events, Abstractions.StreamPosition expectedVersion)
        {
            return AppendToStreamAsync(streamName, events, (long)expectedVersion.Position);
        }

        private async Task AppendToStreamAsync(string streamName, IEnumerable<Event.Abstractions.EventData> events, long expectedVersion)
        {
            var eventStoreData = GetData(events);
            if (eventStoreData.Count > 0)
            {
                try
                {
                    if (await _connection.ConnectWithStatusAsync())
                    {
                        var writeResult = await _connection.AppendToStreamAsync(streamName, expectedVersion, eventStoreData);
                    }
                    else
                    {
                        throw new InvalidOperationException("Event_Store_Not_Connected");
                    }

                }
                catch (EventStore.ClientAPI.Exceptions.WrongExpectedVersionException ex)
                {
                    throw new ConcurrencyException(ex.ExpectedVersion, ex.ActualVersion, ex);
                }

            }
        }


        public async IAsyncEnumerable<ReadEventData> ReadStreamEvents(string streamname, StreamDirection direction, Abstractions.StreamPosition readPosition)
        {
            long sliceStart = (long)readPosition.Position;
            StreamEventsSlice slice;
            if (!await _connection.ConnectWithStatusAsync())
            {
                throw new InvalidOperationException("Event_Store_Not_Connected");
            }
            do
            {
                slice = await _connection.ReadStreamEventsForwardAsync(streamname, sliceStart, 200, false);

                foreach (var eventData in slice.Events)
                {
                    var recordedEvent = eventData.Event;

                    var position = eventData.OriginalPosition ?? Position.Start;

                    var readData = new ReadEventData(
                    id: recordedEvent.EventId,
                    eventType: recordedEvent.EventType,
                    contentType: recordedEvent.IsJson ? "application/json" : "text/plain",
                    metadata: recordedEvent.Metadata,
                    data: recordedEvent.Data,
                    streamPosition: new Abstractions.StreamPosition((ulong)recordedEvent.EventNumber),
                    storePosition: new StorePosition((ulong)position.CommitPosition, (ulong)position.PreparePosition));

                    yield return readData;
                }

                sliceStart = slice.NextEventNumber;

            } while (!slice.IsEndOfStream);

        }

    }
}
