using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Aggregate.Core
{
    public class DirectEventRepository : IDirectEventRepository
    {
        private readonly IEventFactory _factory;
        private readonly IEventStoreStreamConnection _connection;
        private readonly ILogger<DirectEventRepository> _logger;
        public DirectEventRepository(IEventFactory factory, IEventStoreStreamConnection connection, ILogger<DirectEventRepository> logger)
        {
            _factory = factory;
            _connection = connection;
            _logger = logger;
        }

        public Task Save(string stream, IEvent directEvent, IEventMetadata metadata)
        {
            var data = _factory.CreateData(directEvent, metadata);
            _logger.LogInformation("Appending single event {identifier} to {streamName} at any stream position", metadata.ID, stream);
            return _connection.AppendToStreamAsync(stream, new EventData[] { data }, StreamState.Any);
        }

        public Task Save(string stream, DirectEvent direct)
        {
            var data = _factory.CreateData(direct.Event, direct.Metadata);

            _logger.LogInformation("Appending direct event {identifier} to {streamName} at any stream position", direct.Metadata.Identifier, stream);
            return _connection.AppendToStreamAsync(stream, new EventData[] { data }, StreamState.Any);
        }

        public Task Save(string stream, IEnumerable<DirectEvent> events)
        {
            List<EventData> toSend = new List<EventData>();
            foreach (var eventObj in events)
            {
                _logger.LogTrace("Creating event data for {identifier} to be added to {streamName} at any stream position", eventObj.Metadata.Identifier, stream);
                toSend.Add(_factory.CreateData(eventObj.Event, eventObj.Metadata));
            }
            _logger.LogTrace("Appending {eventsCount} events to {streamName} at any stream position", toSend.Count, stream);
            return _connection.AppendToStreamAsync(stream, toSend, StreamState.Any);
        }
    }
}
