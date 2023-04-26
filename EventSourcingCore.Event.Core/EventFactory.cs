using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Core.Reflection;
using Core.Serialization.Json;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Event.Core
{
    public class EventFactory : IEventFactory
    {
        private ILogger<EventFactory> _logger;
        public EventFactory(ILogger<EventFactory> logger)
        {
            _logger = logger;
        }
        public EventFactory()
        {

        }

        public bool TryCreateMetadata<TMetadataType>(byte[] data, out TMetadataType metadata) where TMetadataType : IEventMetadata
        {
            _logger?.LogDebug("Attempting to create {metadataType} from {byteCount} bytes", typeof(TMetadataType), data.Length);
            JsonParser parser = new JsonParser();

            if (parser.TryParse(data, out metadata))
            {
                _logger.LogTrace("Created {metadataType} from {byteCount} bytes", typeof(TMetadataType), data.Length);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to create {metadataType} from {byteCount} bytes", typeof(TMetadataType), data.Length);
                return false;
            }
        }

        public bool TryCreateMetadata(Type metadataType, byte[] data, out IEventMetadata metadata)
        {
            if (metadataType == null)
            {
                metadata = null;
                return false;
            }

            _logger?.LogTrace("Attempting to create {metadataType} from {byteCount} bytes", metadataType.Name, data.Length);
            JsonParser parser = new JsonParser();

            parser.TryParse(metadataType, data, out var untyped);

            if (untyped is IEventMetadata typed)
            {
                _logger.LogTrace("Created {metadataType} from {byteCount} bytes", metadataType.Name, data.Length);
                metadata = typed;
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to create {metadataType} from {byteCount} bytes", metadataType.Name, data.Length);
                metadata = null;
                return false;
            }
        }


        public EventData CreateData(IEvent eventObj, IEventMetadata metadataObj)
        {
            JsonSerializer serializer = new JsonSerializer();
            byte[] metadata = serializer.Serialize(metadataObj);
            byte[] eventData = serializer.Serialize(eventObj);

            _logger?.LogTrace("Creating event data from {identifier} created {byteCount} metadata bytes and {byteCount} event data bytes", metadataObj.Identifier, metadata.Length, eventData.Length);
            return new EventData(metadataObj.ID, metadataObj.Identifier, "application/json", metadata, eventData);
        }

        public bool TryCreateEvent(Type eventType, byte[] data, out IEvent eventObj)
        {

            if (!eventType.ImplementsInterface<IEvent>())
            {
                throw new ArgumentException($"{nameof(eventType)} must implement {nameof(IEvent)}");
            }

            JsonParser parser = new JsonParser();

            if (!parser.TryParse(eventType, data, out object nonTyped))
            {
                _logger.LogWarning("Failed to parse {eventType} from {byteCount} bytes", eventType.Name, data.Length);
                eventObj = null;
                return false;
            }

            if (!(nonTyped is IEvent typed))
            {
                _logger.LogWarning("Failed to parse {eventType} from {byteCount} bytes as the resulting type does not implement IEvent", eventType.Name, data.Length);
                eventObj = null;
                return false;
            }

            eventObj = typed;

            _logger.LogTrace("{eventType} created from {byteCount} bytes", eventType.Name, data.Length);
            return true;
        }

        public bool TryCreateEvent<TEvent>(byte[] data, out TEvent eventObj) where TEvent : IEvent
        {
            JsonParser parser = new JsonParser();
            if (parser.TryParse(data, out eventObj))
            {
                _logger.LogTrace("{eventType} created from {byteCount} bytes", typeof(TEvent).Name, data.Length);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to parse {eventType} from {byteCount} bytes", typeof(TEvent).Name, data.Length);
                return false;
            }
        }

        public bool TryCreateEvent<TEvent>(ReadOnlyMemory<byte> data, out TEvent eventObj) where TEvent : IEvent
        {
            JsonParser parser = new JsonParser();
            if (parser.TryParse(data, out eventObj))
            {
                _logger.LogTrace("{eventType} created from {byteCount} bytes", typeof(TEvent).Name, data.Length);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to parse {eventType} from {byteCount} bytes", typeof(TEvent).Name, data.Length);
                return false;
            }
        }

        public bool TryCreateEvent(Type eventType, ReadOnlyMemory<byte> data, out IEvent eventObj)
        {
            if (!eventType.ImplementsInterface<IEvent>())
            {
                throw new ArgumentException($"{nameof(eventType)} must implement {nameof(IEvent)}");
            }

            JsonParser parser = new JsonParser();

            if (!parser.TryParse(eventType, data, out object nonTyped))
            {
                _logger.LogWarning("Failed to parse {eventType} from {byteCount} bytes", eventType.Name, data.Length);
                eventObj = null;
                return false;
            }

            if (!(nonTyped is IEvent typed))
            {
                _logger.LogWarning("Failed to parse {eventType} from {byteCount} bytes as the resulting type does not implement IEvent", eventType.Name, data.Length);
                eventObj = null;
                return false;
            }

            eventObj = typed;
            _logger.LogTrace("{eventType} created from {byteCount} bytes", eventType.Name, data.Length);
            return true;
        }

        public bool TryCreateMetadata<TMetadata>(ReadOnlyMemory<byte> data, out TMetadata metadata) where TMetadata : IEventMetadata
        {
            JsonParser parser = new JsonParser();

            if (parser.TryParse(data, out metadata))
            {
                _logger.LogTrace("{metadataType} created from {byteCount} bytes", typeof(TMetadata).Name, data.Length);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to parse {metadataType} from {byteCount} bytes", typeof(TMetadata).Name, data.Length);
                return false;
            }

        }

        public bool TryCreateMetadata(Type metadataType, ReadOnlyMemory<byte> data, out IEventMetadata metadata)
        {
            if (metadataType == null)
            {
                metadata = null;
                return false;
            }
            JsonParser parser = new JsonParser();

            parser.TryParse(metadataType, data, out var untyped);

            if (untyped is IEventMetadata typed)
            {
                _logger.LogTrace("{metadataType} created from {byteCount} bytes", metadataType.Name, data.Length);
                metadata = typed;
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to parse {metadataType} from {byteCount} bytes", metadataType.Name, data.Length);
                metadata = null;
                return false;
            }
        }
    }
}
