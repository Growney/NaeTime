using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Aggregate.Core.Tests.Implementations.Events
{
    public class BoolChanged : IEvent
    {
        public bool NewValue { get; set; }
    }
}
