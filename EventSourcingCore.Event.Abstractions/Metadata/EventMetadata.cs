using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Event.Abstractions.Metadata
{
    public class EventMetadata : IEventMetadata
    {
        public EventMetadata(Guid iD, string identifier, ZonedDateTime createdAt, ZonedDateTime validFrom)
        {
            ID = iD;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            CreatedAt = createdAt;
            ValidFrom = validFrom;
        }

        public Guid ID { get; }

        public string Identifier { get; }

        public ZonedDateTime CreatedAt { get; }

        public ZonedDateTime ValidFrom { get; }
    }
}
