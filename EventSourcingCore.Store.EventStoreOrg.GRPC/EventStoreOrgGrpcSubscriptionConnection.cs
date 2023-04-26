using EventStore.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Store.Abstractions;
using StreamPosition = EventStore.Client.StreamPosition;

namespace EventSourcingCore.Store.EventStoreOrg.GRPC
{
    public class EventStoreOrgGrpcSubscriptionConnection : IEventStoreSubscriptionConnection
    {
        private readonly EventStoreClient _client;
        private readonly ILogger _logger;
        public EventStoreOrgGrpcSubscriptionConnection(EventStoreClient client, ILogger<EventStoreOrgGrpcSubscriptionConnection> logger)
        {
            _client = client;
            _logger = logger;
        }


        public async Task<bool> SubscribeToAllAsync(Func<ReadEventData, Task> onEventAppeared, Func<Task<StorePosition?>> position = null)
        {
            StorePosition? startPosition = null;
            if (position != null)
            {
                startPosition = await position();
            }

            StreamSubscription subscription = null;
            if (startPosition.HasValue)
            {
                _logger.LogInformation("GRPC subscribing to ALL from position C:{commitPosition} P:{perparePosition}", startPosition.Value.CommitPosition, startPosition.Value.PreparePosition);

                subscription = await _client.SubscribeToAllAsync(
                start: FromAll.After(new Position(startPosition.Value.CommitPosition, startPosition.Value.PreparePosition)),
                eventAppeared: EventAppeared(onEventAppeared),
                subscriptionDropped: SubscriptionDropped(() => _ = SubscribeToAllAsync(onEventAppeared, position)));
            }
            else
            {
                _logger.LogInformation($"GRPC subscribing to ALL from start");

                subscription = await _client.SubscribeToAllAsync(
                start: FromAll.Start,
                eventAppeared: EventAppeared(onEventAppeared),
                subscriptionDropped: SubscriptionDropped(() => _ = SubscribeToAllAsync(onEventAppeared, position)));
            }

            if (subscription != null)
            {
                _logger?.LogInformation("GRPC subscribed to ALL with subscription id {subscriptionId}", subscription.SubscriptionId);
            }
            else
            {
                _logger?.LogInformation($"GRPC subscribe to ALL returned a null subscription");
            }

            return true;
        }

        public async Task<bool> SubscribeToStreamAsync(string name, Func<ReadEventData, Task> onEventAppeared, Func<Task<Abstractions.StreamPosition?>> position = null)
        {
            Abstractions.StreamPosition? startPosition = null;
            if (position != null)
            {
                startPosition = await position();
            }

            StreamSubscription subscription = null;

            if (startPosition.HasValue)
            {
                _logger?.LogInformation("GRPC subscribing to {streamName} from position P:{streamPosition}", name, startPosition.Value.Position);

                subscription = await _client.SubscribeToStreamAsync(
                streamName: name,
                start: FromStream.After(new StreamPosition(startPosition.Value.Position)),
                eventAppeared: EventAppeared(onEventAppeared),
                subscriptionDropped: SubscriptionDropped(() => _ = SubscribeToStreamAsync(name, onEventAppeared, position)));
            }
            else
            {
                _logger?.LogInformation("GRPC subscribing to {streamName} start", name);

                subscription = await _client.SubscribeToStreamAsync(
               streamName: name,
               start: FromStream.Start,
               eventAppeared: EventAppeared(onEventAppeared),
               subscriptionDropped: SubscriptionDropped(() => _ = SubscribeToStreamAsync(name, onEventAppeared, position)));
            }

            if (subscription != null)
            {
                _logger?.LogInformation("GRPC subscribed to {streamName} with subscription id {subscriptionId}", name, subscription.SubscriptionId);
            }
            else
            {
                _logger?.LogWarning("GRPC subscribe to {streamName} returned a null subscription", name);
            }


            return true;
        }
#nullable enable
        private Action<StreamSubscription, SubscriptionDroppedReason, Exception?> SubscriptionDropped(Action reconnect) =>
        (subscriptions, reason, exception) =>
        {
            _logger?.LogInformation("GRPC subscription {subscriptionId} dropped with reason: {dropReason}", subscriptions.SubscriptionId, reason);
            if (reason != SubscriptionDroppedReason.Disposed)
            {
                _logger?.LogWarning("GRPC subscription {subscriptionId} attempting reconnect", subscriptions.SubscriptionId);
                reconnect();
            }
        };
#nullable disable
        private Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> EventAppeared(Func<ReadEventData, Task> handler) =>
           (subscription, resolvedEvent, token) =>
           {
               var originalEvent = resolvedEvent.OriginalEvent;
               var position = resolvedEvent.OriginalPosition ?? Position.Start;

               var eventData = new ReadEventData(
                   id: originalEvent.EventId.ToGuid(),
                   eventType: originalEvent.EventType,
                   contentType: originalEvent.ContentType,
                   metadata: originalEvent.Metadata,
                   data: originalEvent.Data,
                   streamPosition: new Abstractions.StreamPosition(originalEvent.EventNumber),
                   storePosition: new Abstractions.StorePosition(position.CommitPosition, position.PreparePosition));

               return handler(eventData);
           };


    }
}
