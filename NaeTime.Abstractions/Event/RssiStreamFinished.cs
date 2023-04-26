using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class RssiStreamFinished : IEvent
    {
        public RssiStreamFinished(Guid streamId)
        {
            StreamId = streamId;
        }

        public Guid StreamId { get; }
    }
}
