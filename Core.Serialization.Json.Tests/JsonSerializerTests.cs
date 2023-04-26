using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Core.Serialization.Json.Tests.Implementations;

namespace Core.Serialization.Json.Tests
{
    public class JsonSerializerTests
    {
        private readonly JsonSerializer _serializer = new JsonSerializer();

        [Fact]
        public void For_SerializeObjectToBytes_When_ObjectIsNull_Expect_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.Serialize(null));
        }

        [Fact]
        public void For_SerializeObjectToString_When_ObjectIsNull_Expect_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.SerializeToString(null));
        }
        [Fact]
        public void For_SerializeObjectToBytes_When_ObjectProvided_Expect_DataIsNotNull()
        {
            Assert.NotNull(_serializer.Serialize(new ImmutableBuiltInTypeStruct()));
        }

        [Fact]
        public void For_SerializeObjectToString_When_ObjectProvided_Expect_StringIsNotNull()
        {
            Assert.NotNull(_serializer.SerializeToString(new ImmutableBuiltInTypeStruct()));
        }
        [Fact]
        public void For_SerializeObjectToString_When_ObjectProvided_Expect_StringIsNotEmpty()
        {
            Assert.NotEmpty(_serializer.SerializeToString(new ImmutableBuiltInTypeStruct()));
        }
    }
}
