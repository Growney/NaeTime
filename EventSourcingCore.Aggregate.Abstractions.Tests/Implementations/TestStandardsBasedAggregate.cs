using EventSourcingCore.Aggregate.Abstractions.Tests.Implementations.Events;
using System;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions.Tests.Implementations
{
    public class TestStandardsBasedAggregate : AggregateRoot
    {
        public bool BoolValue { get; private set; } = false;
        public string StringValue { get; private set; } = string.Empty;
        public int IntValue { get; private set; } = 0;

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

        public void When(ExceptionEvent e)
        {
            throw new Exception();
        }
    }
}
