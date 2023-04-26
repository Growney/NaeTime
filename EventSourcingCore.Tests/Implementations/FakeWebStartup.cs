using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingCore.CommandHandler.ASPNET;
using EventSourcingCore.CommandHandler.Core;
using EventSourcingCore.Aggregate.Core;
using EventSourcingCore.Tests.Implementations.Employee;
using EventSourcingCore.Store.Abstractions;
using EventSourcingCore.CommandHandler.Abstractions;
using Core.Security.Abstractions;
using System;
using EventSourcingCore.Event.Core;
using Core.Security.Core;
using Core.Security.ASPNET;

namespace EventSourcingCore.Tests.Implementations
{
    public class FakeWebStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultASPNetCommandRouting();
            services.AddASPNETSecurityCore<FakeStaticKeyProvider>();
            services.AddSingleton<IEventStoreStreamConnection, FakeEventStoreConnection>();
            services.AddDefaultCommandHandlerContainers();
            services.AddAggregateCore();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseJWTAuthentication();
            app.UseASPNetCommandRouting();
        }
    }
}
