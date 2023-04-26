using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Serialization.Json.Tests.Implementations
{
    internal static class TestValues
    {
        public const string NestedJson = "{ \"ChildOne\": " + TestJson + ", \"ChildTwo\": " + TestJson + "}";
        public const string TestJson = "{\"BoolVal\":true,\"ByteVal\":123,\"CharVal\":\"g\",\"DecimalVal\":3.145,\"DoubleVal\":16.839,\"FloatVal\":31.2122,\"IntVal\":-29,\"UIntVal\":30,\"LongVal\":-2147484567,\"ULongVal\":2147484567,\"ShortVal\":-14000,\"UShortVal\":13342,\"StringVal\":\"this is a test string please ignore\"}";
        public const string TestNodaTimeJson = "{\"InstantVal\":\"2019-01-02T03:04:05Z\",\"OffsetDateTimeVal\":\"2012-01-02T03:04:05.123456789Z\",\"ZonedDateTimeVal\":\"2012-10-28T01:30:00+01 Europe/London\",\"LocalDateTimeVal\":\"2012-01-02T03:04:05.123456789\",\"LocalDateVal\":\"2012-01-02\",\"LocalTimeVal\":\"01:02:03.004000567\",\"OffsetVal\":\"+05:30\",\"IntervalVal\":{\"Start\":\"2012-01-02T03:04:05.67Z\",\"End\":\"2013-06-07T08:09:10.123456789Z\"},\"DurationVal\":\"48:00:00\",}";
        public static readonly ImmutableNodaTimeObject TestNodaTimeObject =
            new ImmutableNodaTimeObject(
                Instant.FromUtc(2019, 1, 2, 3, 4, 5),
                new LocalDateTime(2012, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.Zero),
                new ZonedDateTime(new LocalDateTime(2012, 10, 28, 1, 30), DateTimeZoneProviders.Tzdb["Europe/London"], Offset.FromHours(1)),
                new LocalDateTime(2012, 1, 2, 3, 4, 5, CalendarSystem.Iso).PlusNanoseconds(123456789),
                new LocalDate(2012, 1, 2),
                LocalTime.FromHourMinuteSecondMillisecondTick(1, 2, 3, 4, 5).PlusNanoseconds(67),
                Offset.FromHoursAndMinutes(5, 30),
                new Interval(Instant.FromUtc(2012, 1, 2, 3, 4, 5) + Duration.FromMilliseconds(670), Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromNanoseconds(123456789)),
                Duration.FromHours(48)
               );
        public static readonly ImmutableBuiltInTypeObject TestJsonObject =
            new ImmutableBuiltInTypeObject(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");
    }
}
