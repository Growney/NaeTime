using System;

namespace EventSourcingCore.Event.Abstractions
{
    public class EventAttribute : Attribute
    {
        public string Identifier { get; }
        public EventAttribute(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }
            Identifier = identifier;
        }
    }
}
