using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface IAggregateEventHandlerRepository
    {
        IAggregateEventHandler GetHandler(Type aggregateType, string identifier);
    }
}
