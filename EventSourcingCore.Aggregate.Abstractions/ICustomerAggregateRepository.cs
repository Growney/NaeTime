using System;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface ICustomerAggregateRepository : IAggregateRepository
    {
        Task<T> Get<T>(Guid id) where T : IAggregateRoot;
        Task Save(IAggregateRoot aggregateRoot);

    }
}
