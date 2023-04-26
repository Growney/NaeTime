using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Event.Abstractions.Metadata
{
    public class SystemEventMetadata : IEventMetadata
    {
        public SystemEventMetadata(Guid iD, string identifier, ZonedDateTime createdAt, ZonedDateTime validFrom, Guid tokenID)
        {
            ID = iD;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            CreatedAt = createdAt;
            ValidFrom = validFrom;
            TokenID = tokenID;
        }

        public Guid ID { get; }
        public string Identifier { get; }
        public ZonedDateTime CreatedAt { get; }
        public ZonedDateTime ValidFrom { get; }
        public Guid TokenID { get; }
    }
}
