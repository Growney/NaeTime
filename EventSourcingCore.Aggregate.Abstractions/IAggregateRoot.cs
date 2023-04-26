using System;
using System.Collections.Generic;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface IAggregateRoot
    {
        Guid Id { get; }
        ulong? Version { get; set; }
        IEnumerable<RaisedEvent> GetEvents();
        void ClearEvents();
    }
}
