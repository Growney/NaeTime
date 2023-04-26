using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Store.Abstractions
{

    public class ReadEventData : EventData
    {
        public StreamPosition StreamPosition { get; }
        public StorePosition StorePosition { get; }
        public ReadEventData(Guid id, string eventType, string contentType, ReadOnlyMemory<byte> metadata, ReadOnlyMemory<byte> data, StreamPosition streamPosition, StorePosition storePosition)
           : base(id, eventType, contentType, metadata, data)
        {
            StreamPosition = streamPosition;
            StorePosition = storePosition;
        }
    }

}
