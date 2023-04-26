using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Aggregate.Core.Tests.Implementations.Events
{
    public class IntChanged : IEvent
    {
        public int NewValue { get; set; }
    }
}
