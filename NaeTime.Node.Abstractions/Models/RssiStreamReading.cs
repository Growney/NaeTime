using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions.Models
{
    public readonly struct RssiStreamReading
    {
        public RssiStreamReading(Guid streamId, long tick, int value)
        {
            StreamId = streamId;
            Tick = tick;
            Value = value;
        }

        public Guid StreamId { get; }
        public long Tick { get; }
        public int Value { get; }
    }
}
