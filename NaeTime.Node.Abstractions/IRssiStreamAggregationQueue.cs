using NaeTime.Node.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface IRssiStreamAggregationQueue
    {
        bool HasValues { get; }
        void Clear();
        IEnumerable<RssiStreamReading> Dequeue();
        void Enqueue(RssiStreamReading rssiReading);
    }
}
