using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NodaTime;

namespace Core.Serialization.Json.Tests.Implementations
{
    public struct ImmutableNodaTimeStruct
    {
        public ImmutableNodaTimeStruct(Instant instantVal, OffsetDateTime offsetDateTimeVal, ZonedDateTime zonedDateTimeVal, LocalDateTime localDateTimeVal, LocalDate localDateVal, LocalTime localTimeVal, Offset offsetVal, Interval intervalVal, Duration durationVal)
        {
            InstantVal = instantVal;
            OffsetDateTimeVal = offsetDateTimeVal;
            ZonedDateTimeVal = zonedDateTimeVal;
            LocalDateTimeVal = localDateTimeVal;
            LocalDateVal = localDateVal;
            LocalTimeVal = localTimeVal;
            OffsetVal = offsetVal;
            IntervalVal = intervalVal;
            DurationVal = durationVal;
        }

        public Instant InstantVal { get; }
        public OffsetDateTime OffsetDateTimeVal { get; }
        public ZonedDateTime ZonedDateTimeVal { get; }
        public LocalDateTime LocalDateTimeVal { get; }
        public LocalDate LocalDateVal { get; }
        public LocalTime LocalTimeVal { get; }
        public Offset OffsetVal { get; }
        public Interval IntervalVal { get; }
        public Duration DurationVal { get; }
    }
}
