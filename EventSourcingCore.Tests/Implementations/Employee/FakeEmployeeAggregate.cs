using EventSourcingCore.Tests.Implementations.Employee.Event;
using NodaTime;
using System;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Tests.Implementations.Employee
{
    public class FakeEmployeeAggregate : AggregateRoot
    {
        public ZonedDateTime StartDate { get; private set; }
        public ZonedDateTime EndDate { get; private set; }
        public FakeEmployeeAggregate()
        {

        }
        public FakeEmployeeAggregate(Guid employeeID)
        {
            Raise(new FakeEmployeeCreated() { Id = employeeID });
        }


        public void When(FakeEmployeeCreated x)
        {
            Id = x.Id;
        }

        public void SetStartDate(ZonedDateTime dateTime)
        {
            Raise(new FakeEmployeeStartDateSet() { Date = dateTime });
        }

        public void When(FakeEmployeeStartDateSet x)
        {
            StartDate = x.Date;
        }

        public void SetEndDate(ZonedDateTime dateTime)
        {
            if (dateTime.ToInstant() <= StartDate.ToInstant())
            {
                throw new DomainException("EndDate_Before_Start");
            }
            Raise(new FakeEmployeeEndDateSet() { Date = dateTime });
        }

        public void When(FakeEmployeeEndDateSet x)
        {
            EndDate = x.Date;
        }
    }
}
