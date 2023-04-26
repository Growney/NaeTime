using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Aggregate.Abstractions;

namespace EventSourcingCore.Aggregate.Core
{
    public class StreamNameProvider : IStreamNameProvider
    {
        public string GetName(Guid customerId, Guid aggregateId, string aggregateName) => $"{aggregateId}-{customerId}-{aggregateName}";

        public string GetName(Guid customerId, Guid aggregateId, Type aggregateType) => $"{aggregateId}-{customerId}-{aggregateType.Name}";

        public string GetName(Guid aggregateId, string aggregateName) => $"{aggregateId}-{aggregateName}";

        public string GetName(Guid aggregateId, Type aggregateType) => $"{aggregateId}-{aggregateType.Name}";
    }
}
