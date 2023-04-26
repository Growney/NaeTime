using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Aggregate.Abstractions.Tests.Implementations.Events
{
    public class BoolChanged : IEvent
    {
        public bool NewValue { get; set; }
    }
}
