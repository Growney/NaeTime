using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Event.Abstractions
{
    public class EventData
    {
        public Guid ID { get; }
        public string EventType { get; }
        public string ContentType { get; }
        public ReadOnlyMemory<byte> Metadata { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public EventData(Guid id, string eventType, string contentType, ReadOnlyMemory<byte> metadata, ReadOnlyMemory<byte> data)
        {
            ID = id;
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            ContentType = contentType;
            Metadata = metadata;
            Data = data;
        }
    }
}
