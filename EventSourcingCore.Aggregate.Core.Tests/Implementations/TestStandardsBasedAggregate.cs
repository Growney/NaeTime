using EventSourcingCore.Aggregate.Core.Tests.Implementations.Events;
using System;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Core.Tests.Implementations
{
    public class TestStandardsBasedAggregate : AggregateRoot
    {
        public bool BoolValue { get; private set; } = false;
        public string StringValue { get; private set; }
        public int IntValue { get; private set; } = 0;
        public TestStandardsBasedAggregate()
        {

        }
        public TestStandardsBasedAggregate(Guid aggregateID)
        {
            Raise(new AggregateCreated() { Id = aggregateID });
        }

        public void UpdateStringValue(string newValue)
        {
            Raise(new StringChanged() { NewValue = newValue });
        }
        public void UpdateBoolValue(bool newValue)
        {
            Raise(new BoolChanged() { NewValue = newValue });
        }
        public void UpdateIntValue(int newValue)
        {
            Raise(new IntChanged() { NewValue = newValue });
        }

        public void When(AggregateCreated eventObj)
        {
            Id = eventObj.Id;
        }
        public void When(BoolChanged changed)
        {
            BoolValue = changed.NewValue;
        }

        public void When(StringChanged changed)
        {
            StringValue = changed.NewValue;
        }

        public void When(IntChanged changed)
        {
            IntValue = changed.NewValue;
        }
    }
}
