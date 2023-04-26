using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using EventSourcingCore.CommandHandler.Abstractions;
using Xunit;
using System.Linq;
using Core.Reflection;
using EventSourcingCore.CommandHandler.Core.Tests.Implementations;

namespace EventSourcingCore.CommandHandler.Core.Tests
{
    public class StandardsBasedRegistrarTests
    {
        [TestPrecursor]
        private class StandardHandlerClass
        {
            [TestPrecursor]
            public void Handle(CommandMetadata metadata, Command command)
            {

            }
            [TestPrecursor]
            [CommandHandler("Command_v2")]
            public void Handle_v2(CommandMetadata metadata, Command command)
            {

            }
        }
        private readonly IServiceProvider _serviceProvider;
        private readonly IGlobalPrecursorRegistry _globalRegistry;
        public StandardsBasedRegistrarTests()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _globalRegistry = new GlobalPrecursorRegistry(null, _serviceProvider);
            _globalRegistry.RegisterClass<CommandPrecursor>();
        }

        [Fact]
        public void For_Default_Constructor_When_ServiceProviderIsNull_Expect_ThrowsNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() => new StandardsBasedRegistrar(null, _globalRegistry));
        }
        [Fact]
        public void For_StandardHandlerClass_When_CreateClassHandlers_Expect_TwoHandlers()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateClassHandlers<StandardHandlerClass>();

            Assert.Equal(2, handlers.Count());
        }
        [Fact]
        public void For_StandardHandlerObject_When_CreateClassHandlers_Expect_TwoHandlers()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateObjectHandlers(new StandardHandlerClass());

            Assert.Equal(2, handlers.Count());
        }
        [Fact]
        public void For_StandardHandlerClass_When_CreateClassHandlers_Expect_V1Handler()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateClassHandlers<StandardHandlerClass>();

            Assert.Contains(handlers, item =>
            {
                return item.Identifier == "Command_v1";
            });
        }
        [Fact]
        public void For_StandardHandlerObject_When_CreateClassHandlers_Expect_V1Handler()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateObjectHandlers(new StandardHandlerClass());

            Assert.Contains(handlers, item =>
            {
                return item.Identifier == "Command_v1";
            });
        }
        [Fact]
        public void For_StandardHandlerClass_When_CreateClassHandlers_Expect_V2Handler()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateClassHandlers<StandardHandlerClass>();

            Assert.Contains(handlers, item =>
            {
                return item.Identifier == "Command_v2";
            });
        }
        [Fact]
        public void For_StandardHandlerObject_When_CreateClassHandlers_Expect_V2Handler()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateObjectHandlers(new StandardHandlerClass());

            Assert.Contains(handlers, item =>
            {
                return item.Identifier == "Command_v2";
            });
        }
        [Fact]
        public void For_StandardHandlerClass_When_CreateClassHandlers_Expect_OnePrecursor()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateClassHandlers<StandardHandlerClass>();

            Assert.Contains(handlers, item =>
            {
                return item.Identifier == "Command_v2";
            });
        }
        [Fact]
        public void For_StandardHandlerObject_When_CreateClassHandlers_Expect_HandlerV1OnePrecursor()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateObjectHandlers(new StandardHandlerClass());

            ICommandHandler handler = handlers.Single(x =>
            {
                return x.Identifier == "Command_v1";
            });

            Assert.Equal(3, handler.Precursors.Count());
        }
        [Fact]
        public void For_StandardHandlerObject_When_CreateClassHandlers_Expect_HandlerV2OnePrecursor()
        {
            StandardsBasedRegistrar registrar = new StandardsBasedRegistrar(new MethodProviderFactory(_serviceProvider), _globalRegistry);

            IEnumerable<ICommandHandler> handlers = registrar.CreateObjectHandlers(new StandardHandlerClass());

            ICommandHandler handler = handlers.Single(x =>
            {
                return x.Identifier == "Command_v2";
            });

            Assert.Equal(3, handler.Precursors.Count());
        }

    }
}
