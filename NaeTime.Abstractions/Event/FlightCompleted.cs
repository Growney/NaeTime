﻿using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class FlightCompleted : IEvent
    {
        public FlightCompleted(Guid sessionId, int flightId)
        {
            SessionId = sessionId;
            FlightId = flightId;
        }

        public Guid SessionId { get; }
        public int FlightId { get; }
        
    }
}
