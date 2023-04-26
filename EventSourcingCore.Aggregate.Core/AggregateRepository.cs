using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Aggregate.Core
{
    public abstract class AggregateRepository : IAggregateRepository
    {
        private readonly IAggregateEventHandlerRepository _handlerRepository;
        protected readonly IEventStoreStreamConnection _connection;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventFactory _eventfactory;
        protected readonly ILogger<AggregateRepository> _logger;

        public AggregateRepository(IEventStoreStreamConnection connection, IServiceProvider serviceProvider, IAggregateEventHandlerRepository handlerRepository, IEventFactory factory, ILogger<AggregateRepository> logger)
        {
            _connection = connection;
            _serviceProvider = serviceProvider;
            _handlerRepository = handlerRepository;
            _logger = logger;
            _eventfactory = factory;
        }
        public virtual async Task<TAggregate> Get<TAggregate>(string streamName) where TAggregate : IAggregateRoot
        {
            _logger?.LogTrace("Getting {rootType} from {streamName}", typeof(TAggregate).Name, streamName);

            _logger?.LogTrace("Creating aggregate root {rootType}", typeof(TAggregate).Name);
            var root = ActivatorUtilities.CreateInstance<TAggregate>(_serviceProvider);

            var eventStream = _connection.ReadStreamEvents(streamName, StreamDirection.Forward, StreamPosition.Start);

            await foreach (var eventData in eventStream)
            {
                var handler = _handlerRepository.GetHandler(typeof(TAggregate), eventData.EventType);

                if (!_eventfactory.TryCreateEvent(handler.EventType, eventData.Data, out var eventObj))
                {
                    _logger?.LogWarning("Event skipped unabled create event for {identifier}", handler.EventType);
                    continue;
                }
                _logger?.LogTrace("Applying {eventId} with identifier {identifier} to {rootType} aggregate root", eventData.ID, eventData.EventType, typeof(TAggregate).Name);

                try
                {
                    handler.Invoke(root, eventObj);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying event {identifier} when loading aggregate {rootType} from {streamName}", eventData.EventType, typeof(TAggregate).Name, streamName);
                    throw ex;
                }

                if (eventData.StreamPosition.Position != 0)
                {
                    root.Version = eventData.StreamPosition.Position;
                }
                else
                {
                    if (root.Version == null)
                    {
                        root.Version = 0;
                    }
                    else
                    {
                        root.Version++;
                    }

                }
            }
            _logger?.LogInformation("Type {aggregateName} loaded stream {streamName} at {version} with Id {aggregateID}", typeof(TAggregate).Name, streamName, root.Version, root.Id);
            return root;
        }

        protected int Apply(IAggregateRoot root, IEnumerable<RaisedEvent> events)
        {
            int applied = 0;
            foreach (var eventObj in events)
            {
                var rootType = root.GetType();
                var handler = _handlerRepository.GetHandler(rootType, eventObj.Identifier);

                _logger?.LogTrace("Applying event {identifier} to root {rootType}", eventObj.Identifier, rootType.Name);

                try
                {
                    handler.Invoke(root, eventObj.Event);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying event {identifier} to root {rootType} from {streamName}", eventObj.Identifier, rootType.Name);
                }

                applied++;
            }
            return applied;
        }
        public virtual void Apply(IAggregateRoot root)
        {
            var events = root.GetEvents();
            Apply(root, events);
        }

        protected virtual EventData CreateEventdata(string streamName, IEvent eventObj)
        {
            var metadata = CreateMetadata(streamName, eventObj);
            return _eventfactory.CreateData(eventObj, metadata);
        }

        protected Task WriteEventsToStore(string streamName, IEnumerable<RaisedEvent> raisedEvents, ulong? version)
        {
            List<EventData> toSave = new List<EventData>();

            foreach (var raisedEvent in raisedEvents)
            {
                var eventData = CreateEventdata(streamName, raisedEvent.Event);
                toSave.Add(eventData);
            }

            if (toSave.Count > 0)
            {
                if (version.HasValue)
                {
                    _logger.LogInformation("Writing {eventsCount} events to {streamName} at {streamPosition}", toSave.Count, streamName, version.Value);
                    return _connection.AppendToStreamAsync(streamName, toSave, new StreamPosition(version.Value));
                }
                else
                {
                    _logger.LogInformation("Writing {eventsCount} events to {streamName} at the start of the stream", toSave.Count, streamName);
                    return _connection.AppendToStreamAsync(streamName, toSave, StreamState.NoStream);
                }

            }

            return Task.CompletedTask;
        }
        public virtual Task Save(string streamName, IAggregateRoot aggregateRoot)
        {
            _logger.LogInformation("Saving {rootType} to {streamName}", aggregateRoot.GetType().Name, streamName);
            var raisedEvents = aggregateRoot.GetEvents();
            Apply(aggregateRoot, raisedEvents);
            return WriteEventsToStore(streamName, raisedEvents, aggregateRoot.Version);
        }

        protected abstract IEventMetadata CreateMetadata(string streamName, IEvent eventObj);


    }
}
