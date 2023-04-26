using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Event.Abstractions.Metadata
{
    public class StreamEventMetadata : IEventMetadata
    {
        public StreamEventMetadata(Guid iD, string identifier, string streamname, ZonedDateTime createdAt, ZonedDateTime validFrom)
        {
            ID = iD;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Streamname = streamname ?? throw new ArgumentNullException(nameof(streamname));
            CreatedAt = createdAt;
            ValidFrom = validFrom;
        }

        public Guid ID { get; }
        public string Identifier { get; }
        public string Streamname { get; }
        public ZonedDateTime CreatedAt { get; }
        public ZonedDateTime ValidFrom { get; }
    }
}
