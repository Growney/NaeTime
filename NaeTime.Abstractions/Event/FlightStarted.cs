using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class FlightStarted : IEvent
    {
        public FlightStarted(Guid sessionId, int flightId, int frequencyId)
        {
            SessionId = sessionId;
            FlightId = flightId;
            FrequencyId = frequencyId;
        }

        public Guid SessionId { get; }
        public int FlightId { get; }
        public int FrequencyId { get; }
    }
}
