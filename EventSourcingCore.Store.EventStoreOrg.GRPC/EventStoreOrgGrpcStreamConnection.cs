using EventSourcingCore.Store.Abstractions;
using EventStore.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StreamPosition = EventStore.Client.StreamPosition;
using StreamState = EventStore.Client.StreamState;

namespace EventSourcingCore.Store.EventStoreOrg.GRPC
{
    public class EventStoreOrgGrpcStreamConnection : IEventStoreStreamConnection
    {
        private readonly EventStoreClient _client;
        private readonly ILogger<EventStoreOrgGrpcStreamConnection> _logger;
        public EventStoreOrgGrpcStreamConnection(ILogger<EventStoreOrgGrpcStreamConnection> logger, EventStoreClient client)
        {
            _client = client;
            _logger = logger;
        }
        private List<EventData> GetData(IEnumerable<Event.Abstractions.EventData> events)
        {
            var eventStoreData = new List<EventData>();

            if (events != null)
            {
                foreach (var standardEvent in events)
                {
                    eventStoreData.Add(new EventData(
                        eventId: Uuid.FromGuid(standardEvent.ID),
                        type: standardEvent.EventType,
                        contentType: standardEvent.ContentType,
                        data: standardEvent.Data,
                        metadata: standardEvent.Metadata.ToArray()));
                }
            }

            return eventStoreData;
        }

        public async Task AppendToStreamAsync(string streamName, IEnumerable<Event.Abstractions.EventData> events, Abstractions.StreamState streamState)
        {
            var data = GetData(events);
            if (data.Count > 0)
            {
                StreamState state;
                if (streamState == Abstractions.StreamState.StreamExists)
                {
                    state = StreamState.StreamExists;
                }
                else if (streamState == Abstractions.StreamState.NoStream)
                {
                    state = StreamState.NoStream;
                }
                else
                {
                    state = StreamState.Any;
                }

                try
                {
                    _logger.LogInformation("Adding {eventCount} events to {streamName}", data.Count, streamName);
                    _ = await _client.AppendToStreamAsync(streamName, state, data);
                }
                catch (WrongExpectedVersionException ex)
                {
                    throw new ConcurrencyException(ex.ExpectedVersion, ex.ActualVersion, ex.InnerException);
                }
            }
            else
            {

            }

        }
        public async Task AppendToStreamAsync(string streamName, IEnumerable<Event.Abstractions.EventData> events, Abstractions.StreamPosition expectedVersion)
        {
            var data = GetData(events);
            try
            {
                _logger.LogInformation("Adding {eventCount} events to {streamName}", data.Count, streamName);
                _ = await _client.AppendToStreamAsync(streamName, new StreamRevision(expectedVersion.Position), data);
            }
            catch (WrongExpectedVersionException ex)
            {
                throw new ConcurrencyException(ex.ExpectedVersion, ex.ActualVersion, ex.InnerException);
            }
        }

        public async IAsyncEnumerable<ReadEventData> ReadStreamEvents(string streamname, StreamDirection direction, Abstractions.StreamPosition readPosition)
        {

            Direction streamDirection;
            switch (direction)
            {
                default:
                case StreamDirection.Forward:
                    streamDirection = Direction.Forwards;
                    break;
                case StreamDirection.Reverse:
                    streamDirection = Direction.Backwards;
                    break;
            }

            ulong sliceStart = readPosition.Position;

            EventStoreClient.ReadStreamResult slice = _client.ReadStreamAsync(
                    direction: streamDirection,
                    streamName: streamname, new StreamPosition(sliceStart));
            _logger.LogInformation("Reading stream events from stream {streamName}", streamname);
            await foreach (var eventData in slice)
            {
                var recordedEvent = eventData.Event;

                var position = eventData.OriginalPosition ?? Position.Start;

                var readData = new ReadEventData(
                    id: recordedEvent.EventId.ToGuid(),
                    eventType: recordedEvent.EventType,
                    contentType: recordedEvent.ContentType,
                    metadata: recordedEvent.Metadata,
                    data: recordedEvent.Data,
                    streamPosition: new Abstractions.StreamPosition(recordedEvent.EventNumber),
                    storePosition: new Abstractions.StorePosition(position.CommitPosition, position.PreparePosition)
                    );
                _logger.LogTrace("Read {identifier} from stream {streamName}", recordedEvent.EventType, streamname);
                yield return readData;
            }
        }
    }
}
