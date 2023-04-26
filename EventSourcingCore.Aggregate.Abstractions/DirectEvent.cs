using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public struct DirectEvent
    {
        public DirectEvent(IEvent @event, IEventMetadata metadata)
        {
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public IEvent Event { get; }
        public IEventMetadata Metadata { get; }
    }
}
