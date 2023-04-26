using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Event.Abstractions.Metadata
{
    public interface IEventMetadata
    {
        Guid ID { get; }
        string Identifier { get; }
        ZonedDateTime CreatedAt { get; }
        ZonedDateTime ValidFrom { get; }
    }
}
