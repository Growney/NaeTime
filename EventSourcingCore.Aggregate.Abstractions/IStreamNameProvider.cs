using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface IStreamNameProvider
    {
        string GetName(Guid customerId, Guid aggregateId, string aggregateName);
        string GetName(Guid customerId, Guid aggregateId, Type aggregateType);
        string GetName(Guid aggregateId, string aggregateName);
        string GetName(Guid aggregateId, Type aggregateType);
    }
}
