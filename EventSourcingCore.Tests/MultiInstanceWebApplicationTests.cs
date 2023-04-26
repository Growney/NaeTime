using System;
using System.Threading.Tasks;
using Xunit;
using EventSourcingCore.Tests.Implementations.Employee.Command;
using NodaTime;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingCore.Store.Abstractions;
using EventSourcingCore.Aggregate.Abstractions;
using Core.Security.Abstractions;
using EventSourcingCore.Tests.Implementations.Employee;
using EventSourcingCore.Tests.Implementations;

namespace EventSourcingCore.Tests
{
    public class MultiInstanceWebApplicationTests
    {
        private readonly TestWebApplicationFactory<FakeWebStartup> _factory;

        public MultiInstanceWebApplicationTests()
        {
            _factory = new TestWebApplicationFactory<FakeWebStartup>();
        }
        [Fact]
        public async Task For_TestWebStartup_When_InvalidPath_Expect_NotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/notacorrectpath");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task For_TestWebStartup_When_CreateEmployee_Expect_SuccessResponse()
        {
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, new FakeCreateEmployee() { EmployeeID = TestHelper.EmployeeOneID });
        }
        [Fact]
        public async Task For_TestWebStartup_When_ModifyEmployee_Expect_SuccessResponse()
        {
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, new FakeCreateEmployee() { EmployeeID = TestHelper.EmployeeOneID });
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, new FakeSetEmployeeStartDate() { EmployeeID = TestHelper.EmployeeOneID, Date = new ZonedDateTime(Instant.FromDateTimeUtc(new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc)), DateTimeZone.Utc) });
        }

        [Fact]
        public async Task For_TestWebStartup_When_EndDateBeforeStartDate_Expect_DomainException()
        {
            var createEmployeeCommand = new FakeCreateEmployee() { EmployeeID = TestHelper.EmployeeOneID };
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, createEmployeeCommand);
            var setStartDateCommand = new FakeSetEmployeeStartDate() { EmployeeID = TestHelper.EmployeeOneID, Date = new ZonedDateTime(Instant.FromDateTimeUtc(new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc)), DateTimeZone.Utc) };
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, setStartDateCommand);
            var setEndDateCommand = new FakeSetEmployeeEndDate() { EmployeeID = TestHelper.EmployeeOneID, Date = new ZonedDateTime(Instant.FromDateTimeUtc(new DateTime(2020, 09, 10, 0, 0, 0, DateTimeKind.Utc)), DateTimeZone.Utc) };
            await _factory.AssertCommandCode(TestHelper.UserOneID, setEndDateCommand, HttpStatusCode.NotAcceptable, "EndDate_Before_Start");
        }

        [Fact]
        public async Task For_TestWebStartup_When_BadIdentifier_Expect_NotFound()
        {
            var createEmployeeCommand = new FakeCreateEmployee() { EmployeeID = TestHelper.EmployeeOneID };
            var client = _factory.CreateClient();
            var response = await client.PostCommandAsync(TestHelper.UserOneID, createEmployeeCommand, "Bad identifier");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.Equal("Command_Identifier_Not_Found", responseMessage);
        }


        [Fact]
        public async Task For_TestWebStartup_When_CreatingAggregate_Expect_CorrectStreamNameHasEvents()
        {
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, new FakeCreateEmployee() { EmployeeID = TestHelper.EmployeeOneID });

            var connection = _factory.Services.GetService<IEventStoreStreamConnection>();

            int eventCount = 0;
            await foreach (var e in connection.ReadStreamEvents($"{TestHelper.CustomerID}-{TestHelper.EmployeeOneID}-{typeof(FakeEmployeeAggregate).Name}", StreamDirection.Forward, StreamPosition.Start))
            {
                eventCount++;
            }

            Assert.Equal(1, eventCount);
        }
        [Fact]
        public async Task For_TestWebStartup_When_SettingAggregateValues_Expect_CorrectValues()
        {
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, new FakeCreateEmployee() { EmployeeID = TestHelper.EmployeeOneID });
            var startDate = new ZonedDateTime(Instant.FromDateTimeUtc(new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc)), DateTimeZone.Utc);
            var setStartDateCommand = new FakeSetEmployeeStartDate() { EmployeeID = TestHelper.EmployeeOneID, Date = startDate };
            await _factory.AssertCommandSuccess(TestHelper.UserOneID, setStartDateCommand);


            using (var scope = _factory.Services.CreateScope())
            {
                var customerContextAccssor = scope.ServiceProvider.GetService<ICustomerContextAccessor>();
                customerContextAccssor.Context = new CustomerContext(TestHelper.CustomerID);

                var repo = scope.ServiceProvider.GetService<ICustomerAggregateRepository>();
                var aggregate = await repo.Get<FakeEmployeeAggregate>(TestHelper.EmployeeOneID);

                Assert.Equal(TestHelper.EmployeeOneID, aggregate.Id);
                Assert.Equal(startDate, aggregate.StartDate);
                Assert.Equal(new ZonedDateTime(), aggregate.EndDate);
            }
        }
    }
}
