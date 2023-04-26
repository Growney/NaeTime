using Microsoft.Extensions.DependencyInjection;
using System;
using EventSourcingCore.CommandHandler.Abstractions;
using EventSourcingCore.CommandHandler.Core;
using Xunit;
using EventSourcingCore.Event.Core;
using EventSourcingCore.CommandHandler.ASPNET.Tests.Implementations;

namespace EventSourcingCore.CommandHandler.ASPNET.Tests
{
    public class DefaultHttpRequestCommandHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpRequestCommandHandler _requestHandler;
        public DefaultHttpRequestCommandHandlerTests()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddDefaultASPNetCommandRouting();
            serviceCollection.AddTransientCommandHandlerContainer<Implementations.CommandHandler>();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            _requestHandler = _serviceProvider.GetRequiredService<IHttpRequestCommandHandler>();
        }

        [Fact]
        public async void For_HandleRequest_When_NullRequest_Expect_ArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _requestHandler.Handle(null));
        }

        [Fact]
        public async void For_HandleRequest_When_NoMetaData_Expect_BadRequest()
        {
            Microsoft.AspNetCore.Http.HttpContext context = await TestHelper.CreateCommandContext(new Command(123, 5343, 23, "Val", Guid.NewGuid()), nameof(Command));

            await _requestHandler.Handle(context);

            Assert.Equal(400, context.Response.StatusCode);
        }

        [Fact]
        public async void For_HandleRequest_When_ValidRequest_Expect_Success()
        {
            Microsoft.AspNetCore.Http.HttpContext context = await TestHelper.CreateCommandContext(new Command(123, 5343, 23, "Val", Guid.NewGuid()), nameof(Command));
            string metadataString = TestHelper.GetMetadataString(new CommandMetadata(typeof(Command).Name, new NodaTime.ZonedDateTime(), new NodaTime.ZonedDateTime()));
            context.Request.Headers.Add("Tensor-Command-Metadata", new Microsoft.Extensions.Primitives.StringValues(metadataString));

            await _requestHandler.Handle(context);

            Assert.Equal(202, context.Response.StatusCode);
        }
    }
}
