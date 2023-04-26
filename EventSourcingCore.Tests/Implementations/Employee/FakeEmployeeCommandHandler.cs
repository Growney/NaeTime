using System.Threading.Tasks;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.CommandHandler.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Tests.Implementations.Employee.Command;

namespace EventSourcingCore.Tests.Implementations.Employee
{
    [CommandHandlerContainer(CommandHandlerScope.Transient)]
    public class FakeEmployeeCommandHandler
    {
        private readonly ICustomerAggregateRepository _repository;
        public FakeEmployeeCommandHandler(ICustomerAggregateRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(FakeCreateEmployee command)
        {
            var aggregate = new FakeEmployeeAggregate(command.EmployeeID);

            return _repository.Save(aggregate);
        }

        public async Task Handle(FakeSetEmployeeStartDate command)
        {
            var aggregate = await _repository.Get<FakeEmployeeAggregate>(command.EmployeeID);
            aggregate.SetStartDate(command.Date);

            await _repository.Save(aggregate);
        }

        public async Task Handle(FakeSetEmployeeEndDate command)
        {
            var aggregate = await _repository.Get<FakeEmployeeAggregate>(command.EmployeeID);
            aggregate.SetEndDate(command.Date);

            await _repository.Save(aggregate);
        }
    }
}
