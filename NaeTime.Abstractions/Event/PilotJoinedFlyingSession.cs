using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class PilotJoinedFlyingSession : IEvent
    {
        public PilotJoinedFlyingSession(Guid pilotId, Guid sessionId)
        {
            PilotId = pilotId;
            SessionId = sessionId;
        }

        public Guid PilotId { get; }
        public Guid SessionId { get; }
    }
}
