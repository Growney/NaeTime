using System;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.Tests.Implementations.Employee.Command
{
    public class FakeCreateEmployee : ICommand
    {
        public Guid EmployeeID { get; set; }
    }
}
