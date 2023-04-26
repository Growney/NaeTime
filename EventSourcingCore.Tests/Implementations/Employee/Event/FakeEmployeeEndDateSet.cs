using NodaTime;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Tests.Implementations.Employee.Event
{
    public class FakeEmployeeEndDateSet : IEvent
    {
        public ZonedDateTime Date { get; set; }
    }
}
