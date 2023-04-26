using System;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public class EventHandlerAttribute : Attribute
    {
        public string Identifier { get; }
        public EventHandlerAttribute(string identifier)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }
    }
}
