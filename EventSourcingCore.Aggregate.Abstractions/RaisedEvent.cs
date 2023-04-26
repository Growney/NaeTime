using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public class RaisedEvent
    {
        public RaisedEvent(IEvent eventObj, string identifier)
        {
            Event = eventObj ?? throw new ArgumentNullException(nameof(eventObj));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public IEvent Event { get; }
        public string Identifier { get; }
    }
}
