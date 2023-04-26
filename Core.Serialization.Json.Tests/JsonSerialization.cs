using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NodaTime;
using Core.Serialization.Json.Tests.Implementations;

namespace Core.Serialization.Json.Tests
{
    public class JsonSerialization
    {
        private readonly JsonParser _parser = new JsonParser();
        private readonly JsonSerializer _serializer = new JsonSerializer();

        [Fact]
        public void For_SerializeToBytesThenTryParse_When_EmptyObject_Expect_EmptyStruct()
        {
            var toParse = new ImmutableBuiltInTypeStruct();

            var bytes = _serializer.Serialize(toParse);

            _parser.TryParse<ImmutableBuiltInTypeStruct>(bytes, out var result);

            Assert.Equal(toParse, result);
        }

        [Fact]
        public void For_SerializeToBytesThenParse_When_EmptyObject_Expect_EmptyStruct()
        {
            var toParse = new ImmutableBuiltInTypeStruct();

            var bytes = _serializer.Serialize(toParse);

            var parsed = _parser.Parse<ImmutableBuiltInTypeStruct>(bytes);

            Assert.Equal(toParse, parsed);
        }
        [Fact]
        public void For_SerializeToStringThenTryParse_When_EmptyObject_Expect_EmptyStruct()
        {
            var toParse = new ImmutableBuiltInTypeStruct();

            var value = _serializer.SerializeToString(toParse);

            _parser.TryParse<ImmutableBuiltInTypeStruct>(value, out var result);

            Assert.Equal(toParse, result);
        }

        [Fact]
        public void For_SerializeToStingThenParse_When_EmptyObject_Expect_EmptyStruct()
        {
            var toParse = new ImmutableBuiltInTypeStruct();

            var value = _serializer.SerializeToString(toParse);

            var parsed = _parser.Parse<ImmutableBuiltInTypeStruct>(value);

            Assert.Equal(toParse, parsed);
        }

        [Fact]
        public void For_SerializeToBytesThenTryParse_When_ValidObject_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeObject(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var bytes = _serializer.Serialize(toParse);

            _parser.TryParse<ImmutableBuiltInTypeObject>(bytes, out var result);

            Assert.True(result.BoolVal);
            Assert.Equal(toParse.ByteVal, result.ByteVal);
            Assert.Equal(toParse.CharVal, result.CharVal);
            Assert.Equal(toParse.DecimalVal, result.DecimalVal);
            Assert.Equal(toParse.DoubleVal, result.DoubleVal);
            Assert.Equal(toParse.FloatVal, result.FloatVal);
            Assert.Equal(toParse.IntVal, result.IntVal);
            Assert.Equal(toParse.UIntVal, result.UIntVal);
            Assert.Equal(toParse.LongVal, result.LongVal);
            Assert.Equal(toParse.ULongVal, result.ULongVal);
            Assert.Equal(toParse.ShortVal, result.ShortVal);
            Assert.Equal(toParse.UShortVal, result.UShortVal);
            Assert.Equal(toParse.StringVal, result.StringVal);
        }

        [Fact]
        public void For_SerializeToBytesThenParse_When_ValidObject_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeObject(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var bytes = _serializer.Serialize(toParse);

            var parsed = _parser.Parse<ImmutableBuiltInTypeObject>(bytes);

            Assert.True(parsed.BoolVal);
            Assert.Equal(toParse.ByteVal, parsed.ByteVal);
            Assert.Equal(toParse.CharVal, parsed.CharVal);
            Assert.Equal(toParse.DecimalVal, parsed.DecimalVal);
            Assert.Equal(toParse.DoubleVal, parsed.DoubleVal);
            Assert.Equal(toParse.FloatVal, parsed.FloatVal);
            Assert.Equal(toParse.IntVal, parsed.IntVal);
            Assert.Equal(toParse.UIntVal, parsed.UIntVal);
            Assert.Equal(toParse.LongVal, parsed.LongVal);
            Assert.Equal(toParse.ULongVal, parsed.ULongVal);
            Assert.Equal(toParse.ShortVal, parsed.ShortVal);
            Assert.Equal(toParse.UShortVal, parsed.UShortVal);
            Assert.Equal(toParse.StringVal, parsed.StringVal);
        }
        [Fact]
        public void For_SerializeToStringThenTryParse_When_ValidObject_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeObject(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var value = _serializer.SerializeToString(toParse);

            _parser.TryParse<ImmutableBuiltInTypeObject>(value, out var result);

            Assert.True(result.BoolVal);
            Assert.Equal(toParse.ByteVal, result.ByteVal);
            Assert.Equal(toParse.CharVal, result.CharVal);
            Assert.Equal(toParse.DecimalVal, result.DecimalVal);
            Assert.Equal(toParse.DoubleVal, result.DoubleVal);
            Assert.Equal(toParse.FloatVal, result.FloatVal);
            Assert.Equal(toParse.IntVal, result.IntVal);
            Assert.Equal(toParse.UIntVal, result.UIntVal);
            Assert.Equal(toParse.LongVal, result.LongVal);
            Assert.Equal(toParse.ULongVal, result.ULongVal);
            Assert.Equal(toParse.ShortVal, result.ShortVal);
            Assert.Equal(toParse.UShortVal, result.UShortVal);
            Assert.Equal(toParse.StringVal, result.StringVal);
        }

        [Fact]
        public void For_SerializeToStingThenParse_When_ValidObject_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeObject(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var value = _serializer.SerializeToString(toParse);

            var parsed = _parser.Parse<ImmutableBuiltInTypeObject>(value);

            Assert.True(parsed.BoolVal);
            Assert.Equal(toParse.ByteVal, parsed.ByteVal);
            Assert.Equal(toParse.CharVal, parsed.CharVal);
            Assert.Equal(toParse.DecimalVal, parsed.DecimalVal);
            Assert.Equal(toParse.DoubleVal, parsed.DoubleVal);
            Assert.Equal(toParse.FloatVal, parsed.FloatVal);
            Assert.Equal(toParse.IntVal, parsed.IntVal);
            Assert.Equal(toParse.UIntVal, parsed.UIntVal);
            Assert.Equal(toParse.LongVal, parsed.LongVal);
            Assert.Equal(toParse.ULongVal, parsed.ULongVal);
            Assert.Equal(toParse.ShortVal, parsed.ShortVal);
            Assert.Equal(toParse.UShortVal, parsed.UShortVal);
            Assert.Equal(toParse.StringVal, parsed.StringVal);
        }

        [Fact]
        public void For_SerializeToBytesThenTryParse_When_ValidStruct_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeStruct(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var bytes = _serializer.Serialize(toParse);

            _parser.TryParse<ImmutableBuiltInTypeStruct>(bytes, out var result);

            Assert.True(result.BoolVal);
            Assert.Equal(toParse.ByteVal, result.ByteVal);
            Assert.Equal(toParse.CharVal, result.CharVal);
            Assert.Equal(toParse.DecimalVal, result.DecimalVal);
            Assert.Equal(toParse.DoubleVal, result.DoubleVal);
            Assert.Equal(toParse.FloatVal, result.FloatVal);
            Assert.Equal(toParse.IntVal, result.IntVal);
            Assert.Equal(toParse.UIntVal, result.UIntVal);
            Assert.Equal(toParse.LongVal, result.LongVal);
            Assert.Equal(toParse.ULongVal, result.ULongVal);
            Assert.Equal(toParse.ShortVal, result.ShortVal);
            Assert.Equal(toParse.UShortVal, result.UShortVal);
            Assert.Equal(toParse.StringVal, result.StringVal);
        }

        [Fact]
        public void For_SerializeToBytesThenParse_When_ValidStruct_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeStruct(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var bytes = _serializer.Serialize(toParse);

            var parsed = _parser.Parse<ImmutableBuiltInTypeStruct>(bytes);

            Assert.True(parsed.BoolVal);
            Assert.Equal(toParse.ByteVal, parsed.ByteVal);
            Assert.Equal(toParse.CharVal, parsed.CharVal);
            Assert.Equal(toParse.DecimalVal, parsed.DecimalVal);
            Assert.Equal(toParse.DoubleVal, parsed.DoubleVal);
            Assert.Equal(toParse.FloatVal, parsed.FloatVal);
            Assert.Equal(toParse.IntVal, parsed.IntVal);
            Assert.Equal(toParse.UIntVal, parsed.UIntVal);
            Assert.Equal(toParse.LongVal, parsed.LongVal);
            Assert.Equal(toParse.ULongVal, parsed.ULongVal);
            Assert.Equal(toParse.ShortVal, parsed.ShortVal);
            Assert.Equal(toParse.UShortVal, parsed.UShortVal);
            Assert.Equal(toParse.StringVal, parsed.StringVal);
        }
        [Fact]
        public void For_SerializeToStringThenTryParse_When_ValidStruct_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeStruct(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var value = _serializer.SerializeToString(toParse);

            _parser.TryParse<ImmutableBuiltInTypeStruct>(value, out var result);

            Assert.True(result.BoolVal);
            Assert.Equal(toParse.ByteVal, result.ByteVal);
            Assert.Equal(toParse.CharVal, result.CharVal);
            Assert.Equal(toParse.DecimalVal, result.DecimalVal);
            Assert.Equal(toParse.DoubleVal, result.DoubleVal);
            Assert.Equal(toParse.FloatVal, result.FloatVal);
            Assert.Equal(toParse.IntVal, result.IntVal);
            Assert.Equal(toParse.UIntVal, result.UIntVal);
            Assert.Equal(toParse.LongVal, result.LongVal);
            Assert.Equal(toParse.ULongVal, result.ULongVal);
            Assert.Equal(toParse.ShortVal, result.ShortVal);
            Assert.Equal(toParse.UShortVal, result.UShortVal);
            Assert.Equal(toParse.StringVal, result.StringVal);
        }

        [Fact]
        public void For_SerializeToStingThenParse_When_ValidStruct_Expect_CorrectValues()
        {
            var toParse = new ImmutableBuiltInTypeStruct(true, 123, 'g', 3.145m, 16.839, 31.2122f, -29, 30, -2147484567, 2147484567, -14000, 13342, "this is a test string please ignore");

            var value = _serializer.SerializeToString(toParse);

            var parsed = _parser.Parse<ImmutableBuiltInTypeStruct>(value);

            Assert.True(parsed.BoolVal);
            Assert.Equal(toParse.ByteVal, parsed.ByteVal);
            Assert.Equal(toParse.CharVal, parsed.CharVal);
            Assert.Equal(toParse.DecimalVal, parsed.DecimalVal);
            Assert.Equal(toParse.DoubleVal, parsed.DoubleVal);
            Assert.Equal(toParse.FloatVal, parsed.FloatVal);
            Assert.Equal(toParse.IntVal, parsed.IntVal);
            Assert.Equal(toParse.UIntVal, parsed.UIntVal);
            Assert.Equal(toParse.LongVal, parsed.LongVal);
            Assert.Equal(toParse.ULongVal, parsed.ULongVal);
            Assert.Equal(toParse.ShortVal, parsed.ShortVal);
            Assert.Equal(toParse.UShortVal, parsed.UShortVal);
            Assert.Equal(toParse.StringVal, parsed.StringVal);
        }
    }
}
