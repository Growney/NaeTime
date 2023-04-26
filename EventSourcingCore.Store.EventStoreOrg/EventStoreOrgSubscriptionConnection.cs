using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public class EventStoreOrgSubscriptionConnection : IEventStoreSubscriptionConnection
    {
        private readonly IEventStoreConnectionWithStatus _connection;
        public EventStoreOrgSubscriptionConnection(IEventStoreConnectionWithStatus connection)
        {
            _connection = connection;
        }

        public async Task<bool> SubscribeToAllAsync(Func<ReadEventData, Task> onEventAppeared, Func<Task<StorePosition?>> position = null)
        {
            StorePosition? startPosition = null;
            if (position != null)
            {
                startPosition = await position();
            }

            if (!startPosition.HasValue)
            {
                return await SubscribeToAllFromStartAsync(onEventAppeared);
            }
            else
            {
                return await SubscribeToAllFromAsync(onEventAppeared, startPosition.Value);
            }
        }
        public async Task<bool> SubscribeToStreamAsync(string name, Func<ReadEventData, Task> onEventAppeared, Func<Task<Abstractions.StreamPosition?>> position = null)
        {
            Abstractions.StreamPosition? startPosition = null;
            if (position != null)
            {
                startPosition = await position();
            }

            if (!startPosition.HasValue)
            {
                return await SubscribeToStreamFromStart(name, onEventAppeared);
            }
            else
            {
                return await SubscribeToStreamFrom(name, onEventAppeared, startPosition.Value);
            }
        }
        public async Task<bool> SubscribeToAllFromAsync(Func<ReadEventData, Task> onEventAppeared, StorePosition position)
        {
            if (!await _connection.ConnectWithStatusAsync())
            {
                return false;
            }
            _connection.SubscribeToAllFrom(new Position((long)position.CommitPosition, (long)position.PreparePosition), CatchUpSubscriptionSettings.Default, eventAppeared: EventAppeared(onEventAppeared));
            return true;
        }

        public async Task<bool> SubscribeToAllFromStartAsync(Func<ReadEventData, Task> onEventAppeared)
        {
            if (!await _connection.ConnectWithStatusAsync())
            {
                return false;
            }
            _connection.SubscribeToAllFrom(Position.Start, CatchUpSubscriptionSettings.Default, eventAppeared: EventAppeared(onEventAppeared));
            return true;
        }



        public async Task<bool> SubscribeToStreamFrom(string name, Func<ReadEventData, Task> onEventAppeared, Abstractions.StreamPosition position)
        {
            if (!await _connection.ConnectWithStatusAsync())
            {
                return false;
            }
            _connection.SubscribeToStreamFrom(name, (long)position.Position, CatchUpSubscriptionSettings.Default, eventAppeared: EventAppeared(onEventAppeared));
            return true;
        }

        public async Task<bool> SubscribeToStreamFromStart(string name, Func<ReadEventData, Task> onEventAppeared)
        {
            if (!await _connection.ConnectWithStatusAsync())
            {
                return false;
            }
            _connection.SubscribeToStreamFrom(name, StreamCheckpoint.StreamStart, CatchUpSubscriptionSettings.Default, eventAppeared: EventAppeared(onEventAppeared));
            return true;

        }

        private Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> EventAppeared(Func<ReadEventData, Task> handler) =>
            (subscription, resolvedEvent) =>
            {
                var originalEvent = resolvedEvent.OriginalEvent;

                var position = resolvedEvent.OriginalPosition ?? Position.Start;

                var eventData = new ReadEventData(
                    id: originalEvent.EventId,
                    eventType: originalEvent.EventType,
                    contentType: originalEvent.IsJson ? "application/json" : "text/plain",
                    metadata: originalEvent.Metadata,
                    data: originalEvent.Data,
                    streamPosition: new Abstractions.StreamPosition((ulong)originalEvent.EventNumber),
                    storePosition: new StorePosition((ulong)position.CommitPosition, (ulong)position.PreparePosition));

                if (eventData.EventType.StartsWith('$'))
                {
                    return Task.CompletedTask;
                }
                return handler(eventData);
            };





    }
}
