using EventStore.Client;
using NaeTime.Server.Abstractions.Consumers;
using NaeTime.Server.EventStore.Consumers;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEventStoreConsumers(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider =>
        {
            var conn = EventStoreClientSettings.Create("esdb://admin:changeit@127.0.0.1:2113?tls=false&keepAliveTimeout=10000&keepAliveInterval=10000");
            return new EventStoreClient(conn);
        });

        services.AddTransient<INodeConsumer, EventStoreNodeConsumer>();

        return services;
    }
}
