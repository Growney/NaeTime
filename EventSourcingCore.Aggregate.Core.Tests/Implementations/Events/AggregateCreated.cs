using System;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Aggregate.Core.Tests.Implementations.Events
{
    public class AggregateCreated : IEvent
    {
        public Guid Id { get; set; }
    }
}
