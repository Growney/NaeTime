using System;
using Xunit;
using NodaTime;
using Core.Serialization.Json.Tests.Implementations;

namespace Core.Serialization.Json.Tests
{
    public class JsonParserGenericParseStringTests
    {
        private readonly JsonParser _parser = new JsonParser();

        [Fact]
        public void For_GenericParseString_When_StringIsNull_Expect_ThrowsArgumentNullException()
        {
            string data = null;
            Assert.Throws<ArgumentNullException>(() => _parser.Parse<ImmutableBuiltInTypeObject>(data));
        }
        [Fact]
        public void For_GenericParseString_When_StringIsEmpty_Expect_ThrowsArgumentException()
        {
            string data = string.Empty;
            Assert.Throws<ArgumentException>(() => _parser.Parse<ImmutableBuiltInTypeObject>(data));
        }
        [Fact]
        public void For_GenericParseString_When_StringIsBad_Expect_ThrowsFormatException()
        {
            string data = "not json string";
            Assert.Throws<FormatException>(() => _parser.Parse<ImmutableBuiltInTypeObject>(data));
        }

        [Fact]
        public void For_GenericParseString_When_EmptyJsonObject_Expect_EmptyStruct()
        {
            string emptyJson = "{}";
            var parsed = _parser.Parse<ImmutableBuiltInTypeStruct>(emptyJson);
            Assert.Equal(new ImmutableBuiltInTypeStruct(), parsed);
        }

        [Fact]
        public void For_GenericParseString_When_BuiltInJsonObject_Expect_CorrectValues()
        {
            var parsed = _parser.Parse<ImmutableBuiltInTypeObject>(TestValues.TestJson);

            Assert.True(parsed.BoolVal);
            Assert.Equal(TestValues.TestJsonObject.ByteVal, parsed.ByteVal);
            Assert.Equal(TestValues.TestJsonObject.CharVal, parsed.CharVal);
            Assert.Equal(TestValues.TestJsonObject.DecimalVal, parsed.DecimalVal);
            Assert.Equal(TestValues.TestJsonObject.DoubleVal, parsed.DoubleVal);
            Assert.Equal(TestValues.TestJsonObject.FloatVal, parsed.FloatVal);
            Assert.Equal(TestValues.TestJsonObject.IntVal, parsed.IntVal);
            Assert.Equal(TestValues.TestJsonObject.UIntVal, parsed.UIntVal);
            Assert.Equal(TestValues.TestJsonObject.LongVal, parsed.LongVal);
            Assert.Equal(TestValues.TestJsonObject.ULongVal, parsed.ULongVal);
            Assert.Equal(TestValues.TestJsonObject.ShortVal, parsed.ShortVal);
            Assert.Equal(TestValues.TestJsonObject.UShortVal, parsed.UShortVal);
            Assert.Equal(TestValues.TestJsonObject.StringVal, parsed.StringVal);
        }
        [Fact]
        public void For_GenericParseString_When_BuiltInJsonStruct_Expect_CorrectValues()
        {
            var parsed = _parser.Parse<ImmutableBuiltInTypeStruct>(TestValues.TestJson);

            Assert.True(parsed.BoolVal);
            Assert.Equal(TestValues.TestJsonObject.ByteVal, parsed.ByteVal);
            Assert.Equal(TestValues.TestJsonObject.CharVal, parsed.CharVal);
            Assert.Equal(TestValues.TestJsonObject.DecimalVal, parsed.DecimalVal);
            Assert.Equal(TestValues.TestJsonObject.DoubleVal, parsed.DoubleVal);
            Assert.Equal(TestValues.TestJsonObject.FloatVal, parsed.FloatVal);
            Assert.Equal(TestValues.TestJsonObject.IntVal, parsed.IntVal);
            Assert.Equal(TestValues.TestJsonObject.UIntVal, parsed.UIntVal);
            Assert.Equal(TestValues.TestJsonObject.LongVal, parsed.LongVal);
            Assert.Equal(TestValues.TestJsonObject.ULongVal, parsed.ULongVal);
            Assert.Equal(TestValues.TestJsonObject.ShortVal, parsed.ShortVal);
            Assert.Equal(TestValues.TestJsonObject.UShortVal, parsed.UShortVal);
            Assert.Equal(TestValues.TestJsonObject.StringVal, parsed.StringVal);
        }



        [Fact]
        public void For_GenericParseString_When_NestedObjects_Expect_CorrectValues()
        {
            var parsed = _parser.Parse<ImmutableParentObject>(TestValues.NestedJson);

            var childOne = parsed.ChildOne;
            Assert.True(childOne.BoolVal);
            Assert.Equal(TestValues.TestJsonObject.ByteVal, childOne.ByteVal);
            Assert.Equal(TestValues.TestJsonObject.CharVal, childOne.CharVal);
            Assert.Equal(TestValues.TestJsonObject.DecimalVal, childOne.DecimalVal);
            Assert.Equal(TestValues.TestJsonObject.DoubleVal, childOne.DoubleVal);
            Assert.Equal(TestValues.TestJsonObject.FloatVal, childOne.FloatVal);
            Assert.Equal(TestValues.TestJsonObject.IntVal, childOne.IntVal);
            Assert.Equal(TestValues.TestJsonObject.UIntVal, childOne.UIntVal);
            Assert.Equal(TestValues.TestJsonObject.LongVal, childOne.LongVal);
            Assert.Equal(TestValues.TestJsonObject.ULongVal, childOne.ULongVal);
            Assert.Equal(TestValues.TestJsonObject.ShortVal, childOne.ShortVal);
            Assert.Equal(TestValues.TestJsonObject.UShortVal, childOne.UShortVal);
            Assert.Equal(TestValues.TestJsonObject.StringVal, childOne.StringVal);

            var childTwo = parsed.ChildTwo;
            Assert.True(childOne.BoolVal);
            Assert.Equal(TestValues.TestJsonObject.ByteVal, childTwo.ByteVal);
            Assert.Equal(TestValues.TestJsonObject.CharVal, childTwo.CharVal);
            Assert.Equal(TestValues.TestJsonObject.DecimalVal, childTwo.DecimalVal);
            Assert.Equal(TestValues.TestJsonObject.DoubleVal, childTwo.DoubleVal);
            Assert.Equal(TestValues.TestJsonObject.FloatVal, childTwo.FloatVal);
            Assert.Equal(TestValues.TestJsonObject.IntVal, childTwo.IntVal);
            Assert.Equal(TestValues.TestJsonObject.UIntVal, childTwo.UIntVal);
            Assert.Equal(TestValues.TestJsonObject.LongVal, childTwo.LongVal);
            Assert.Equal(TestValues.TestJsonObject.ULongVal, childTwo.ULongVal);
            Assert.Equal(TestValues.TestJsonObject.ShortVal, childTwo.ShortVal);
            Assert.Equal(TestValues.TestJsonObject.UShortVal, childTwo.UShortVal);
            Assert.Equal(TestValues.TestJsonObject.StringVal, childTwo.StringVal);
        }
        [Fact]
        public void For_GenericParseString_When_NodaTimeJsonObject_Expect_CorrectValues()
        {
            var parsed = _parser.Parse<ImmutableNodaTimeObject>(TestValues.TestNodaTimeJson);

            Assert.Equal(TestValues.TestNodaTimeObject.InstantVal, parsed.InstantVal);
            Assert.Equal(TestValues.TestNodaTimeObject.OffsetDateTimeVal, parsed.OffsetDateTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.ZonedDateTimeVal, parsed.ZonedDateTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.LocalDateTimeVal, parsed.LocalDateTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.LocalDateVal, parsed.LocalDateVal);
            Assert.Equal(TestValues.TestNodaTimeObject.LocalTimeVal, parsed.LocalTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.OffsetVal, parsed.OffsetVal);
            Assert.Equal(TestValues.TestNodaTimeObject.IntervalVal, parsed.IntervalVal);
            Assert.Equal(TestValues.TestNodaTimeObject.DurationVal, parsed.DurationVal);
        }
        [Fact]
        public void For_GenericParseString_When_NodaTimeJsonStruct_Expect_CorrectValues()
        {
            var parsed = _parser.Parse<ImmutableNodaTimeStruct>(TestValues.TestNodaTimeJson);

            Assert.Equal(TestValues.TestNodaTimeObject.InstantVal, parsed.InstantVal);
            Assert.Equal(TestValues.TestNodaTimeObject.OffsetDateTimeVal, parsed.OffsetDateTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.ZonedDateTimeVal, parsed.ZonedDateTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.LocalDateTimeVal, parsed.LocalDateTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.LocalDateVal, parsed.LocalDateVal);
            Assert.Equal(TestValues.TestNodaTimeObject.LocalTimeVal, parsed.LocalTimeVal);
            Assert.Equal(TestValues.TestNodaTimeObject.OffsetVal, parsed.OffsetVal);
            Assert.Equal(TestValues.TestNodaTimeObject.IntervalVal, parsed.IntervalVal);
            Assert.Equal(TestValues.TestNodaTimeObject.DurationVal, parsed.DurationVal);
        }
    }
}

