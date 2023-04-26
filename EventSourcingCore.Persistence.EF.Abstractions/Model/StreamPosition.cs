using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Persistence.EF.Abstractions.Model
{
    [NonCustomerEntity]
    public class StreamPosition
    {
        public string Key { get; set; }
        public string StreamName { get; set; }
        public ulong Position { get; set; }
    }
}
