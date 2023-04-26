using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Aggregate.Core.Tests.Implementations
{
    public class TestEventStoreConnection : IEventStoreStreamConnection
    {
        private readonly ConcurrentDictionary<string, List<ReadEventData>> _storage = new ConcurrentDictionary<string, List<ReadEventData>>();
        public Task AppendToStreamAsync(string streamName, IEnumerable<EventData> events, StreamState expectedState)
        {

            if (expectedState == StreamState.Any)
            {
                AddEventData(streamName, events);
            }
            else if (expectedState == StreamState.NoStream)
            {
                if (_storage.TryGetValue(streamName, out _))
                {
                    throw new ConcurrencyException(null, 0);
                }
                else
                {
                    AddEventData(streamName, events);
                }
            }
            else if (expectedState == StreamState.StreamExists)
            {
                if (_storage.TryGetValue(streamName, out var data))
                {
                    if (data.Count == 0)
                    {
                        AddEventData(streamName, events);
                    }
                }

                throw new ConcurrencyException(null, 0);
            }

            return Task.CompletedTask;
        }
        private void AddEventData(string streamName, IEnumerable<EventData> events)
        {
            var list = _storage.GetOrAdd(streamName, key =>
            {
                return new List<ReadEventData>();
            });

            foreach (var eventObj in events)
            {
                var count = list.Count;
                list.Add(new ReadEventData(eventObj.ID, eventObj.EventType, eventObj.ContentType, eventObj.Metadata, eventObj.Data, new StreamPosition((ulong)count), new StorePosition((ulong)count, (ulong)count)));
            }
        }
        public Task AppendToStreamAsync(string streamName, IEnumerable<EventData> events, StreamPosition expectedVersion)
        {

            if (_storage.TryGetValue(streamName, out var data))
            {
                if ((long)expectedVersion.Position != data.Count - 1)
                {
                    throw new ConcurrencyException((long)expectedVersion.Position, data.Count);
                }
            }

            AddEventData(streamName, events);

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<ReadEventData> ReadStreamEvents(string streamname, StreamDirection direction, StreamPosition position)
        {
            _storage.TryGetValue(streamname, out var list);

            for (int currentVersion = (int)position.Position; currentVersion < list.Count; currentVersion++)
            {
                await Task.Delay(1);
                yield return list[currentVersion];
            }

        }
    }
}
