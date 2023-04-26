using EventStore.ClientAPI;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public class EventStoreOrgPersistentSubscriptionConnection : IEventStorePersistentSubscriptionConnection
    {
        private readonly IEventStoreConnectionWithStatus _connection;
        private readonly EventStoreConnectionOptions _options;
        public EventStoreOrgPersistentSubscriptionConnection(IEventStoreConnectionWithStatus connection, IOptions<EventStoreConnectionOptions> options)
        {
            _options = options.Value;
            _connection = connection;
        }
        public async Task<bool> ConnectToPersistentSubscription(string stream, string groupName, int bufferSize, Func<ReadEventData, Task<ProjectionEventResult>> onEventAppeared)
        {
            if (!await _connection.ConnectWithStatusAsync())
            {
                return false;
            }
            try
            {
                var connectionResult = await _connection.ConnectToPersistentSubscriptionAsync(stream, groupName,
                eventAppeared: EventAppeared(onEventAppeared),
                bufferSize: bufferSize);
            }
            catch (AggregateException)
            {
                if (_options.AllowPersistentSubscriptionCreation)
                {
                    var settings = PersistentSubscriptionSettings.Create().DoNotResolveLinkTos().StartFromBeginning();

                    await _connection.CreatePersistentSubscriptionAsync(stream, groupName, settings, new EventStore.ClientAPI.SystemData.UserCredentials(_options.CreationUsername, _options.CreationPassword));

                    return await ConnectToPersistentSubscription(stream, groupName, bufferSize, onEventAppeared);
                }

            }

            return true;
        }

        private Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> EventAppeared(Func<ReadEventData, Task<ProjectionEventResult>> onEventAppeared) =>
            async (subscription, resolvedEvent, count) =>
            {
                var originalEvent = resolvedEvent.Event;

                var position = resolvedEvent.OriginalPosition ?? Position.Start;
                var eventData = new ReadEventData(
                    id: originalEvent.EventId,
                    eventType: originalEvent.EventType,
                    contentType: originalEvent.IsJson ? "application/json" : "text/plain",
                    metadata: originalEvent.Metadata,
                    data: originalEvent.Data,
                    streamPosition: new Abstractions.StreamPosition((ulong)originalEvent.EventNumber),
                    storePosition: new StorePosition((ulong)position.CommitPosition, (ulong)position.PreparePosition));
                try
                {
                    var result = await onEventAppeared(eventData);
                    if (result.Acknowledge)
                    {
                        subscription.Acknowledge(resolvedEvent);
                    }
                    else
                    {
                        subscription.Fail(resolvedEvent, ConvertToEventStoreNak(result.Action), result.Reason);
                    }
                }
                catch (Exception ex)
                {
                    subscription.Fail(resolvedEvent, PersistentSubscriptionNakEventAction.Park, $"Exception occured: {ex.GetType()} - {ex.Message}");
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
