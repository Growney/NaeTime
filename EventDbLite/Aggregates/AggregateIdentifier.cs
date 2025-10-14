using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDbLite.Aggregates;
public class AggregateIdentifier
{
    private readonly string _identifier;

    public AggregateIdentifier(string identifier)
    {
        _identifier = identifier;
    }

    public string GetId() => _identifier;

    public static implicit operator AggregateIdentifier(Guid id) => new (id.ToString("N"));
    public static implicit operator AggregateIdentifier(string id) => new (id);
}
