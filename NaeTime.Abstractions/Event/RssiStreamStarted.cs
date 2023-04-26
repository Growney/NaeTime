using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class RssiStreamStarted : IEvent
    {
        public RssiStreamStarted(Guid streamId)
        {
            StreamId = streamId;
        }

        public Guid StreamId { get; }
    }
}
