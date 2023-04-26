using EventSourcingCore.Event.Abstractions;
namespace EventSourcingCore.Aggregate.Core.Tests.Implementations.Events
{
    public class StringChanged : IEvent
    {
        public string NewValue { get; set; }
    }
}
