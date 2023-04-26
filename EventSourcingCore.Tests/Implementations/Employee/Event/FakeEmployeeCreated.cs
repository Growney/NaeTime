using System;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Tests.Implementations.Employee.Event
{
    public class FakeEmployeeCreated : IEvent
    {
        public Guid Id { get; set; }
    }
}
