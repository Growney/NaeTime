using System;
using EventSourcingCore.Event.Abstractions;
using NodaTime;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Aggregate.Core.Factories;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Core.Tests.Implementations
{
    public class TestMetadataFactory : IEventMetadataFactory<SystemEventMetadata>
    {
        public SystemEventMetadata CreateMetadata(IEvent eventObj)
        {
            ZonedClock zonedClock = new ZonedClock(SystemClock.Instance, DateTimeZoneProviders.Tzdb.GetSystemDefault(), CalendarSystem.Iso);
            string identifier = eventObj.GetIdentifier();
            ZonedDateTime userTime = zonedClock.GetCurrentZonedDateTime();
            return new SystemEventMetadata(Guid.NewGuid(), identifier, userTime, userTime, Guid.Empty);
        }
    }
}
