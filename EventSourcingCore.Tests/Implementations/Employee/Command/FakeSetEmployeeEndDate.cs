using NodaTime;
using System;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.Tests.Implementations.Employee.Command
{
    public class FakeSetEmployeeEndDate : ICommand
    {
        public Guid EntityID => EmployeeID;

        public Guid EmployeeID { get; set; }
        public ZonedDateTime Date { get; set; }
    }
}
