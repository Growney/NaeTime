using System;
using Xunit;
using NodaTime;
using Core.Serialization.Json.Tests.Implementations;

namespace Core.Serialization.Json.Tests
{
    public class JsonParserTryParseBytesTests
    {
        private readonly JsonParser _parser = new JsonParser();

        [Fact]
        public void For_TryParseBytes_When_BinaryDataIsNull_Expect_False()
        {
            byte[] data = null;
            Assert.False(_parser.TryParse(typeof(ImmutableBuiltInTypeObject), data, out var _));
        }
        [Fact]
        public void For_TryParseBytes_When_BinaryDataIsEmpty_Expect_False()
        {
            byte[] data = new byte[0];
            Assert.False(_parser.TryParse(typeof(ImmutableBuiltInTypeObject), data, out var _));
        }
        [Fact]
        public void For_TryParseBytes_When_BinaryDataIsBad_Expect_False()
        {
            byte[] data = { 12, 5, 77, 124, 123 };
            Assert.False(_parser.TryParse(typeof(ImmutableBuiltInTypeObject), data, out var _));
        }

        [Fact]
        public void For_TryParseBytes_When_EncodedEmptyJsonObject_Expect_True()
        {
            string emptyJson = "{}";
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(emptyJson);
            Assert.True(_parser.TryParse(typeof(ImmutableBuiltInTypeObject), encoded, out var _));
        }

        [Fact]
        public void For_TryParseBytes_When_EncodedBuiltInJsonObject_Expect_True()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestJson);
            Assert.True(_parser.TryParse(typeof(ImmutableBuiltInTypeObject), encoded, out var _));
        }
        [Fact]
        public void For_TryParseBytes_When_EncodedBuiltInJsonObject_Expect_CorrectType()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestJson);
            _parser.TryParse(typeof(ImmutableBuiltInTypeObject), encoded, out var parsed);
            Assert.IsType<ImmutableBuiltInTypeObject>(parsed);
        }
        [Fact]
        public void For_TryParseBytes_When_EncodedBuiltInJsonStruct_Expect_True()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestJson);
            Assert.True(_parser.TryParse(typeof(ImmutableBuiltInTypeStruct), encoded, out var _));
        }
        [Fact]
        public void For_TryParseBytes_When_EncodedBuiltInJsonStruct_Expect_CorrectType()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestJson);
            _parser.TryParse(typeof(ImmutableBuiltInTypeStruct), encoded, out var parsed);
            Assert.IsType<ImmutableBuiltInTypeStruct>(parsed);
        }
        [Fact]
        public void For_TryParseBytes_When_EncodedBuiltInJsonObject_Expect_CorrectValues()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestJson);
            _parser.TryParse(typeof(ImmutableBuiltInTypeObject), encoded, out var parsedObject);

            var parsed = parsedObject as ImmutableBuiltInTypeObject;

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
        public void For_TryParseBytes_When_EncodedBuiltInJsonStruct_Expect_CorrectValues()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestJson);

            _parser.TryParse(typeof(ImmutableBuiltInTypeStruct), encoded, out var parsedStruct);

            var parsed = (ImmutableBuiltInTypeStruct)parsedStruct;

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
        public void For_TryParseBytes_When_NestedObjects_Expect_CorrectValues()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.NestedJson);
            _parser.TryParse(typeof(ImmutableParentObject), encoded, out var parsedObject);

            var parsed = parsedObject as ImmutableParentObject;

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
        public void For_TryParseBytes_When_EncodedNodaTimeJsonObject_Expect_CorrectValues()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestNodaTimeJson);
            _parser.TryParse(typeof(ImmutableNodaTimeObject), encoded, out var parsedObject);

            var parsed = parsedObject as ImmutableNodaTimeObject;

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
        public void For_TryParseBytes_When_EncodedNodaTimeJsonStruct_Expect_CorrectValues()
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(TestValues.TestNodaTimeJson);
            _parser.TryParse(typeof(ImmutableNodaTimeStruct), encoded, out var parsedStruct);

            var parsed = (ImmutableNodaTimeStruct)parsedStruct;

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
