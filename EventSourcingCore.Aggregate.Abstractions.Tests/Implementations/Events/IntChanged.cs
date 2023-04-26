using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Aggregate.Abstractions.Tests.Implementations.Events
{
    public class IntChanged : IEvent
    {
        public int NewValue { get; set; }
    }
}
