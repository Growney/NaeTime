using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class RssiStreamAssignedToFlight : IEvent
    {
        public RssiStreamAssignedToFlight(Guid flightId, Guid streamId)
        {
            FlightId = flightId;
            StreamId = streamId;
        }

        public Guid FlightId { get; }
        public Guid StreamId { get; }
    }
}
