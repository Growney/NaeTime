using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public abstract class AggregateRoot : IAggregateRoot
    {
        public Guid Id { get; protected set; }
        public ulong? Version { get; set; } = null;

        private readonly List<RaisedEvent> _events = new List<RaisedEvent>();

        protected void Raise(IEvent eventObj, string identifier = null)
        {
            if (eventObj == null)
            {
                throw new ArgumentNullException(nameof(eventObj));
            }
            if (string.IsNullOrWhiteSpace(identifier))
            {
                identifier = eventObj.GetIdentifier();
            }
            _events.Add(new RaisedEvent(eventObj, identifier));
        }
        public void ClearEvents()
        {
            _events.Clear();
        }
        public IEnumerable<RaisedEvent> GetEvents()
        {
            return _events;
        }
    }
}
