using EventSourcingCore.Aggregate.Core.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Aggregate.Core
{
    public class SystemAggregateRepository : AggregateRepository, ISystemAggregateRepository
    {
        private readonly IEventMetadataFactory<SystemEventMetadata> _metadataFactory;
        private readonly IStreamNameProvider _nameProvider;
        public SystemAggregateRepository(IStreamNameProvider nameProvider, IEventMetadataFactory<SystemEventMetadata> metadataFactory, IEventStoreStreamConnection connection, IAggregateEventHandlerRepository handlerRepository, IServiceProvider serviceProvider, IEventFactory eventFactory, ILogger<SystemAggregateRepository> logger)
               : base(connection, serviceProvider, handlerRepository, eventFactory, logger)
        {
            _metadataFactory = metadataFactory;
            _nameProvider = nameProvider;
        }

        public Task<T> Get<T>(Guid id) where T : IAggregateRoot
        {
            var requiredType = typeof(T);
            var streamName = GetStreamName(requiredType, id);
            _logger.LogTrace("Getting system aggregate {aggregateId} from {streamName}", id, streamName);
            return Get<T>(streamName);
        }

        private string GetStreamName(Type aggregateType, Guid aggregateID)
        {
            return _nameProvider.GetName(aggregateID, aggregateType);
        }

        public Task Save(IAggregateRoot aggregateRoot)
        {
            var raisedEvents = aggregateRoot.GetEvents();
            Apply(aggregateRoot, raisedEvents);
            var streamName = GetStreamName(aggregateRoot.GetType(), aggregateRoot.Id);
            _logger.LogInformation("Saving system aggregate {aggregateId} {rootType} to {streamName}", aggregateRoot.Id, aggregateRoot.GetType().Name, streamName);
            return WriteEventsToStore(streamName, raisedEvents, aggregateRoot.Version);
        }

        protected override IEventMetadata CreateMetadata(string streamName, IEvent eventObj)
        {
            return _metadataFactory.CreateMetadata(eventObj);
        }
    }
}
