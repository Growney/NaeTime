using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Aggregate.Core;
using EventSourcingCore.Aggregate.Core.Tests.Implementations;
using EventSourcingCore.Event.Core;
using EventSourcingCore.Aggregate.Core.Tests.Implementations.Events;
using EventSourcingCore.Aggregate.Core.Factories;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Aggregate.Abstractions;
using Core.Reflection.Abstractions;
using Core.Reflection;
using EventSourcingCore.Aggregate.Core.Tests.Implementations;

namespace EventSourcingCore.Aggregate.Core.Tests
{
    public class SystemAggregateRepositoryTests
    {
        private readonly SystemAggregateRepository _repository;

        public SystemAggregateRepositoryTests()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<IEventMetadataFactory<SystemEventMetadata>, TestMetadataFactory>();
            var eventFactory = new EventFactory();

            var provider = services.BuildServiceProvider();
            var methodFactory = new MethodProviderFactory(provider);
            var repository = new StandardAggregateEventHandlerRepository(methodFactory);
            repository.RegisterAggregate(typeof(TestStandardsBasedAggregate));

            _repository = new SystemAggregateRepository(new StreamNameProvider(), new TestMetadataFactory(), new TestEventStoreConnection(), repository, provider, eventFactory, null);
        }


        [Fact]
        public async void For_SavingAndReloadingAggregate_When_NoEvents_Expect_LoadEmptyObject()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);
            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.False(loaded.BoolValue);
            Assert.Null(loaded.StringValue);
            Assert.Equal(0, loaded.IntValue);
        }
        [Fact]
        public async void For_SavingAndReloadingAggregate_When_StringUpdate_Expect_LoadCorrectStringValue()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);
            aggregate.UpdateStringValue("Test string");

            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.False(loaded.BoolValue);
            Assert.Equal("Test string", loaded.StringValue);
            Assert.Equal(0, loaded.IntValue);
        }

        [Fact]
        public async void For_SavingAndReloadingAggregate_When_BoolUpdate_Expect_LoadCorrectBoolValue()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);
            aggregate.UpdateBoolValue(true);

            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.True(loaded.BoolValue);
            Assert.Null(loaded.StringValue);
            Assert.Equal(0, loaded.IntValue);
        }
        [Fact]
        public async void For_SavingAndReloadingAggregate_When_IntUpdate_Expect_LoadCorrectIntValue()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);
            aggregate.UpdateIntValue(1337);
            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.False(loaded.BoolValue);
            Assert.Null(loaded.StringValue);
            Assert.Equal(1337, loaded.IntValue);
        }

        [Fact]
        public async void For_SavingAndReloadingAggregate_When_StringStream_Expect_LoadLastStringValue()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);

            aggregate.UpdateStringValue("Test string one");
            aggregate.UpdateStringValue("Test string two");
            aggregate.UpdateStringValue("Test string three");
            aggregate.UpdateStringValue("Test string four");
            aggregate.UpdateStringValue("Test string five");

            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.False(loaded.BoolValue);
            Assert.Equal("Test string five", loaded.StringValue);
            Assert.Equal(0, loaded.IntValue);
        }

        [Fact]
        public async void For_SavingAndReloadingAggregate_When_IntStream_Expect_LoadLastIntValue()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);

            aggregate.UpdateIntValue(7001);
            aggregate.UpdateIntValue(2323);
            aggregate.UpdateIntValue(38367);
            aggregate.UpdateIntValue(46159);
            aggregate.UpdateIntValue(7070);

            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.False(loaded.BoolValue);
            Assert.Null(loaded.StringValue);
            Assert.Equal(7070, loaded.IntValue);
        }

        [Fact]
        public async void For_SavingAndReloadingAggregate_When_BoolStream_Expect_LoadLastIntValue()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);

            aggregate.UpdateBoolValue(true);
            aggregate.UpdateBoolValue(false);
            aggregate.UpdateBoolValue(true);
            aggregate.UpdateBoolValue(false);
            aggregate.UpdateBoolValue(true);

            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.True(loaded.BoolValue);
            Assert.Null(loaded.StringValue);
            Assert.Equal(0, loaded.IntValue);
        }

        [Fact]
        public async void For_SavingAndReloadingAggregate_When_UpdateStream_Expect_LoadValues()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);

            aggregate.UpdateBoolValue(false);
            aggregate.UpdateStringValue("Test string two");
            aggregate.UpdateIntValue(7001);
            aggregate.UpdateStringValue("Test string three");
            aggregate.UpdateBoolValue(false);
            aggregate.UpdateStringValue("Test string one");
            aggregate.UpdateIntValue(2323);
            aggregate.UpdateBoolValue(true);
            aggregate.UpdateIntValue(38367);
            aggregate.UpdateStringValue("Test string four");
            aggregate.UpdateBoolValue(true);
            aggregate.UpdateIntValue(46159);
            aggregate.UpdateStringValue("Test string five");
            aggregate.UpdateBoolValue(true);
            aggregate.UpdateIntValue(7070);

            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.True(loaded.BoolValue);
            Assert.Equal("Test string five", loaded.StringValue);
            Assert.Equal(7070, loaded.IntValue);
        }
        [Fact]
        public async void For_DoubleSavingAndReloadingAggregate_When_UpdateStream_Expect_LoadValues()
        {
            var aggregateGuid = new Guid("efab4e17-b78d-4251-a2f3-ee17f6c906dc");
            var aggregate = new TestStandardsBasedAggregate(aggregateGuid);

            aggregate.UpdateBoolValue(false);
            aggregate.UpdateStringValue("Test string two");
            aggregate.UpdateIntValue(7001);
            aggregate.UpdateStringValue("Test string three");
            aggregate.UpdateBoolValue(false);
            aggregate.UpdateStringValue("Test string one");
            aggregate.UpdateIntValue(2323);
            aggregate.UpdateBoolValue(true);
            aggregate.UpdateIntValue(38367);
            aggregate.UpdateStringValue("Test string four");
            aggregate.UpdateBoolValue(true);
            aggregate.UpdateIntValue(46159);
            aggregate.UpdateStringValue("Test string five");
            aggregate.UpdateBoolValue(true);
            aggregate.UpdateIntValue(7070);

            await _repository.Save(aggregate);

            var loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            loaded.UpdateBoolValue(false);
            loaded.UpdateStringValue("Not a Test string two");
            loaded.UpdateIntValue(30223);
            loaded.UpdateStringValue("Not a Test string three");
            loaded.UpdateBoolValue(false);
            loaded.UpdateStringValue("Not a Test string one");
            loaded.UpdateIntValue(40228);
            loaded.UpdateBoolValue(true);
            loaded.UpdateIntValue(6150);
            loaded.UpdateStringValue("Not a Test string four");
            loaded.UpdateBoolValue(true);
            loaded.UpdateIntValue(6648);
            loaded.UpdateStringValue("Not a Test string five");
            loaded.UpdateBoolValue(false);
            loaded.UpdateIntValue(11110);

            await _repository.Save(loaded);

            loaded = await _repository.Get<TestStandardsBasedAggregate>(aggregateGuid);

            Assert.Equal(aggregateGuid, loaded.Id);
            Assert.False(loaded.BoolValue);
            Assert.Equal("Not a Test string five", loaded.StringValue);
            Assert.Equal(11110, loaded.IntValue);
        }
    }
}
