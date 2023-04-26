using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface ISystemAggregateRepository : IAggregateRepository
    {
        Task<T> Get<T>(Guid id) where T : IAggregateRoot;
        Task Save(IAggregateRoot aggregateRoot);
    }
}
