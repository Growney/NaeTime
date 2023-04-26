using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface IAggregateEventHandler
    {
        string Identifier { get; }
        Type EventType { get; }
        public void Invoke(object instance, IEvent context);
    }
}
