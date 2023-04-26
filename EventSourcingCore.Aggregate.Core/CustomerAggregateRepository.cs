using EventSourcingCore.Aggregate.Core.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Security.Abstractions;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Aggregate.Core
{
    public class CustomerAggregateRepository : AggregateRepository, ICustomerAggregateRepository
    {
        private readonly ICustomerContextAccessor _customerContextAccessor;
        private readonly IEventMetadataFactory<CustomerEventMetadata> _customerEventMetadataFactory;
        private readonly IStreamNameProvider _nameProvider;
        public CustomerAggregateRepository(IStreamNameProvider nameProvider, IEventMetadataFactory<CustomerEventMetadata> customerEventMetadataFactory, IAggregateEventHandlerRepository handlerRepo, IEventStoreStreamConnection connection, IServiceProvider serviceProvider, IEventFactory eventFactory, ICustomerContextAccessor customerContextAccessor, ILogger<CustomerAggregateRepository> logger)
            : base(connection, serviceProvider, handlerRepo, eventFactory, logger)
        {
            _nameProvider = nameProvider;
            _customerContextAccessor = customerContextAccessor ?? throw new ArgumentNullException(nameof(customerContextAccessor));
            _customerEventMetadataFactory = customerEventMetadataFactory ?? throw new ArgumentNullException(nameof(customerEventMetadataFactory));
        }

        public Task<T> Get<T>(Guid id) where T : IAggregateRoot
        {
            var requiredType = typeof(T);
            var streamName = GetStreamName(requiredType, id);
            _logger.LogTrace("Getting customer aggregate {aggregateId} from {streamName}", id, streamName);
            return Get<T>(streamName);
        }

        private string GetStreamName(Type aggregateType, Guid aggregateID)
        {
            if (_customerContextAccessor != null)
            {
                var customerGuid = _customerContextAccessor.Context.CustomerID;

                if (customerGuid != Guid.Empty)
                {
                    return _nameProvider.GetName(customerGuid, aggregateID, aggregateType);
                }
                else
                {
                    _logger.LogWarning("No customer Id provided when getting stream name for {rootType} for {aggregateID}", aggregateType.GetType().Name, aggregateID);
                }
            }
            throw new UnauthorizedAccessException("No_customer_context_provided");

        }

        public Task Save(IAggregateRoot aggregateRoot)
        {
            var raisedEvents = aggregateRoot.GetEvents();
            Apply(aggregateRoot, raisedEvents);
            var streamName = GetStreamName(aggregateRoot.GetType(), aggregateRoot.Id);
            _logger.LogInformation("Saving customer aggregate {aggregateId} {rootType} to {streamName}", aggregateRoot.Id, aggregateRoot.GetType().Name, streamName);
            return WriteEventsToStore(streamName, raisedEvents, aggregateRoot.Version);
        }

        protected override IEventMetadata CreateMetadata(string streamName, IEvent eventObj)
        {
            return _customerEventMetadataFactory.CreateMetadata(eventObj);
        }
    }
}
