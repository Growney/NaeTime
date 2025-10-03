using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDbLite.Events;
public class AggregateIdentified
{
    public required string Id { get; init; }
}
