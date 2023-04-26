using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class StreamRssiValueLogged : IEvent
    {
        public StreamRssiValueLogged(Guid streamId, long tick, int value)
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
