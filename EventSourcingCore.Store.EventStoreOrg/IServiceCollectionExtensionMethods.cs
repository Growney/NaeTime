using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Readiness.Abstractions;
using EventSourcingCore.Store.Abstractions;
using Microsoft.Extensions.Options;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddEventStoreOrg(this IServiceCollection collection)
        {
            collection.AddOptions<EventStoreConnectionOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("EventStore").Bind(options);
            });

            collection.TryAddSingleton<IEventStoreConnectionWithStatus>(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IOptions<EventStoreConnectionOptions>>();
                return new WrappedEventStoreOrgConnection(configuration.Value.ConnectionString);
            });
            collection.TryAddSingleton<IEventStoreConnection>(serviceProvider =>
            {
                return serviceProvider.GetService<IEventStoreConnectionWithStatus>();
            });

            collection.TryAddSingleton<IEventStorePersistentSubscriptionConnection, EventStoreOrgPersistentSubscriptionConnection>();
            collection.TryAddSingleton<IEventStoreStreamConnection, EventStoreOrgStreamConnection>();
            collection.TryAddSingleton<IEventStoreSubscriptionConnection, EventStoreOrgSubscriptionConnection>();
            collection.TryAddSingleton<IEventStreamConnection, EventStoreStreamConnection>();
            collection.AddSingleton<IReadinessCheck, EventStoreConnectionReadiness>();

            return collection;
        }


    }
}
