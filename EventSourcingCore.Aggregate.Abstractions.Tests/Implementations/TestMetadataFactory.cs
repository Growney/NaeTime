using System;
using EventSourcingCore.Event.Abstractions;
using NodaTime;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Aggregate.Core.Factories;

namespace EventSourcingCore.Aggregate.Abstractions.Tests.Implementations
{
    public class TestMetadataFactory : IEventMetadataFactory<CustomerEventMetadata>
    {
        public CustomerEventMetadata CreateMetadata(IEvent eventObj)
        {
            ZonedClock zonedClock = new ZonedClock(SystemClock.Instance, DateTimeZoneProviders.Tzdb.GetSystemDefault(), CalendarSystem.Iso);
            string identifier = eventObj.GetIdentifier();
            ZonedDateTime userTime = zonedClock.GetCurrentZonedDateTime();
            return new CustomerEventMetadata(Guid.NewGuid(), identifier, userTime, userTime, Guid.NewGuid(), Guid.NewGuid());
        }
    }
}
