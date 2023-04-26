using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Readiness.Abstractions;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Store.EventStoreOrg.GRPC
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddEventStoreOrgGRPC(this IServiceCollection collection)
        {
            collection.AddOptions<EventStoreConnectionOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("EventStore").Bind(options);
            });
            collection.AddSingleton(services =>
            {
                var options = services.GetService<IOptions<EventStoreConnectionOptions>>();
                var config = options.Value;


                var settings = new EventStoreClientSettings()
                {
                    LoggerFactory = services.GetService<ILoggerFactory>(),
                    ConnectionName = $"{config.ConnectionName}-{Guid.NewGuid()}",
                    ConnectivitySettings = {
                        Address = new Uri(config.Address)
                    },
                    DefaultCredentials = new UserCredentials(config.Username, config.Password),
                    CreateHttpMessageHandler = () =>
                        new SocketsHttpHandler
                        {
                            SslOptions =
                            {
                                RemoteCertificateValidationCallback = delegate
                                {
                                    return true;
                                }
                            }
                        }
                };
                return settings;
            });
            collection.AddSingleton(services =>
            {
                var options = services.GetService<EventStoreClientSettings>();

                return new EventStorePersistentSubscriptionsClient(options);
            });
            collection.AddSingleton(services =>
            {
                var options = services.GetService<EventStoreClientSettings>();

                return new EventStoreClient(options);
            });

            collection.TryAddSingleton<IEventStorePersistentSubscriptionConnection, EventStoreOrgGrpcPersistentSubscriptionConnection>();
            collection.TryAddSingleton<IEventStoreStreamConnection, EventStoreOrgGrpcStreamConnection>();
            collection.TryAddSingleton<IEventStoreSubscriptionConnection, EventStoreOrgGrpcSubscriptionConnection>();
            collection.TryAddSingleton<IEventStreamConnection, EventStoreStreamConnection>();

            return collection;
        }
    }
}
