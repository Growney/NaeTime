using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Aggregate.Core
{
    public class StreamAggregateRepository : AggregateRepository, IStreamAggregateRepository
    {
        public StreamAggregateRepository(IEventStoreStreamConnection connection, IServiceProvider serviceProvider, IAggregateEventHandlerRepository repo, IEventFactory eventFactory, ILogger<StreamAggregateRepository> logger)
            : base(connection, serviceProvider, repo, eventFactory, logger)
        {
        }
        protected override IEventMetadata CreateMetadata(string streamName, IEvent eventObj)
        {
            Guid eventID = Guid.NewGuid();
            string identifier = eventObj.GetIdentifier();

            ZonedClock zonedClock = new ZonedClock(SystemClock.Instance, DateTimeZoneProviders.Tzdb.GetSystemDefault(), CalendarSystem.Iso);
            ZonedDateTime systemTime = zonedClock.GetCurrentZonedDateTime();

            return new StreamEventMetadata(eventID, identifier, streamName, systemTime, systemTime);
        }
    }
}
