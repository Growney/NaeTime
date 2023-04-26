using EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public class EventStoreOrgGrpcPersistentSubscriptionConnection : IEventStorePersistentSubscriptionConnection
    {
        private readonly EventStorePersistentSubscriptionsClient _client;
        private readonly EventStoreConnectionOptions _options;
        private readonly ILogger _logger;
        public EventStoreOrgGrpcPersistentSubscriptionConnection(EventStorePersistentSubscriptionsClient client,
            IOptions<EventStoreConnectionOptions> options,
            ILogger<EventStoreOrgGrpcPersistentSubscriptionConnection> logger)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
        }
        public async Task<bool> ConnectToPersistentSubscription(string stream, string groupName, int bufferSize, Func<ReadEventData, Task<ProjectionEventResult>> onEventAppeared)
        {
            PersistentSubscription subscription = null;
            try
            {
                _logger.LogInformation("Connecting to persistent subscription on {streamName} - {groupName} with buffer size {bufferSize}", stream, groupName, bufferSize);
                subscription = await _client.SubscribeToStreamAsync(
                streamName: stream,
                groupName: groupName,
                eventAppeared: EventAppeared(onEventAppeared),
                bufferSize: bufferSize);
            }
            catch (PersistentSubscriptionNotFoundException)
            {
                _logger.LogWarning("Persistent subscription on {streamName} - {groupName} does not exist", stream, groupName);
                if (_options.AllowPersistentSubscriptionCreation)
                {
                    _logger.LogInformation("Creating persistent subscription on {streamName} - {groupName} with buffer size {bufferSize}", stream, groupName, bufferSize);
                    var settings = new PersistentSubscriptionSettings(
                        resolveLinkTos: true,
                        startFrom: StreamPosition.Start);

                    await _client.CreateAsync(stream, groupName, settings);

                    _logger.LogInformation("Connecting to persistent subscription on {streamName} - {groupName} with buffer size {bufferSize}", stream, groupName, bufferSize);
                    subscription = await _client.SubscribeToStreamAsync(
                    streamName: stream,
                    groupName: groupName,
                    eventAppeared: EventAppeared(onEventAppeared),
                    bufferSize: bufferSize);
                }
            }


            return !string.IsNullOrWhiteSpace(subscription?.SubscriptionId);
        }
        private Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> EventAppeared(Func<ReadEventData, Task<ProjectionEventResult>> onEventAppeared) =>
           async (subscription, resolvedEvent, token, count) =>
           {
               var originalEvent = resolvedEvent.Event;
               var position = resolvedEvent.OriginalPosition ?? Position.Start;

               var eventData = new ReadEventData(
                    id: originalEvent.EventId.ToGuid(),
                    eventType: originalEvent.EventType,
                    contentType: originalEvent.ContentType,
                    metadata: originalEvent.Metadata,
                    data: originalEvent.Data,
                    streamPosition: new Abstractions.StreamPosition(originalEvent.EventNumber),
                    storePosition: new Abstractions.StorePosition(position.CommitPosition, position.PreparePosition)
                    );

               try
               {
                   var result = await onEventAppeared(eventData);
                   if (result.Acknowledge)
                   {
                       await subscription.Ack(resolvedEvent);
                   }
                   else
                   {
                       await subscription.Nack(ConvertToEventStoreNak(result.Action), result.Reason, resolvedEvent);
                   }
               }
               catch (Exception ex)
               {
                   await subscription.Nack(PersistentSubscriptionNakEventAction.Park, $"Exception occured: {ex.GetType()} - {ex.Message}", resolvedEvent);
               }

           };
        private PersistentSubscriptionNakEventAction ConvertToEventStoreNak(NakAction action)
        {
            switch (action)
            {
                case NakAction.Park:
                    return PersistentSubscriptionNakEventAction.Park;
                case NakAction.Retry:
                    return PersistentSubscriptionNakEventAction.Retry;
                case NakAction.Skip:
                    return PersistentSubscriptionNakEventAction.Skip;
                case NakAction.Stop:
                    return PersistentSubscriptionNakEventAction.Stop;
                default:
                    break;
            }
            return PersistentSubscriptionNakEventAction.Unknown;
        }
    }
}
