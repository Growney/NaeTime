using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Aggregate.Abstractions.Tests.Implementations.Events
{
    public class StringChanged : IEvent
    {
        public string NewValue { get; set; }
    }
}
