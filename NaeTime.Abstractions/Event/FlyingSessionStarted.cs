using EventSourcingCore.Event.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Event
{
    public class FlyingSessionStarted : IEvent
    {

        public FlyingSessionStarted(Guid flyingSessionId,Guid hostId,Guid locationId)
        {
            HostId = hostId;
            FlyingSessionId = flyingSessionId;
            LocationId = locationId;
        }
        public Guid FlyingSessionId { get; }
        public Guid HostId { get; }
        public Guid LocationId { get; }
    }
}
