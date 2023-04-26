using Microsoft.AspNetCore.Http;
using System;
using Xunit;
using System.IO;
using EventSourcingCore.CommandHandler.ASPNET.Tests.Implementations;

namespace EventSourcingCore.CommandHandler.ASPNET.Tests
{
    public class HttpExtensionMethodsTests
    {
        [Fact]
        public async void For_HttpRequest_When_CorrectlyFormatted_Expect_CorrectValues()
        {
            Command toBeSent = new Command(1374, 1232112, 1235.4f, "This is not a string", new Guid("561f4a9a-cd26-418c-96cb-53273a46e197"));
            HttpContext context = await TestHelper.CreateCommandContext(toBeSent, nameof(Command));

            Command command = await context.Request.GetCommand(typeof(Command)) as Command;

            Assert.Equal(toBeSent.UIntVal, command.UIntVal);
            Assert.Equal(toBeSent.LongVal, command.LongVal);
            Assert.Equal(toBeSent.FloatVal, command.FloatVal);
            Assert.Equal(toBeSent.StringVal, command.StringVal);
            Assert.Equal(toBeSent.Guid, command.Guid);

        }

        [Fact]
        public async void For_HttpRequest_When_EmptyBodyStreamRequest_Expect_InvalidOperationException()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream();

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.Request.GetCommand(typeof(Command)));
        }

        [Fact]
        public async void For_HttpRequest_When_NullBodyStreamRequest_Expect_ArgumentNullException()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Body = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => context.Request.GetCommand(typeof(Command)));
        }
    }
}
