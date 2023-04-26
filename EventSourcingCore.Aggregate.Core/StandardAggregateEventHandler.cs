using System;
using System.Collections.Generic;
using System.Text;
using Core.Reflection.Abstractions;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;

namespace EventSourcingCore.Aggregate.Core
{
    public class StandardAggregateEventHandler : IAggregateEventHandler
    {
        public string Identifier { get; }
        public Type EventType { get; }

        private readonly IStandardActionMethod<IEvent> _standardMethod;

        public StandardAggregateEventHandler(string identifier, Type eventType, IStandardActionMethod<IEvent> standardMethod)
        {
            Identifier = identifier;
            EventType = eventType;

            _standardMethod = standardMethod;
        }

        public void Invoke(object against, IEvent obj)
        {
            _standardMethod.Invoke(against, obj);
        }
    }
}
