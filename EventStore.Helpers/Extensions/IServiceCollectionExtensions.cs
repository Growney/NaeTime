using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventStore.Helpers.Extensions;
public static class IServiceCollectionExtensions
{
    private class HandlerDescriptor
    {
        public HandlerDescriptor(Func<object> instanceFactory, Type handler, string? streamName = null)
        {
            InstanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
            HandlerType = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Func<object> InstanceFactory { get; }
        public Type HandlerType { get; }
        public string? StreamName { get; }
    }
    public static IServiceCollection AddEventHandlers(this IServiceCollection services, Action<IHandlerCollection>? configure = null)
    {
        services.AddSingleton(serviceProvider =>
        {
            var conn = EventStoreClientSettings.Create("esdb://admin:changeit@127.0.0.1:2113?tls=false&keepAliveTimeout=10000&keepAliveInterval=10000");
            return new EventStoreClient(conn);
        });
        services.TryAddSingleton(provider =>
        {
            var collection = provider.GetRequiredService<IHandlerCollection>();
            return collection.BuildProvider();
        });
        services.AddHostedService<HandlerService>();
        services.TryAddSingleton<IHandlerCollection>(provider =>
        {
            var eventStoreClient = provider.GetRequiredService<EventStoreClient>();

            var collection = new HandlerCollection(eventStoreClient);

            configure?.Invoke(collection);

            var descriptors = provider.GetRequiredService<IEnumerable<HandlerDescriptor>>();

            foreach (var descriptor in descriptors)
            {
                collection.Add(descriptor.HandlerType, descriptor.InstanceFactory, descriptor.StreamName);
            }

            return collection;
        });

        return services;
    }

    public static IServiceCollection AddTransientHandler<THandler>(this IServiceCollection services, string? streamName = null)
        where THandler : class, IHandlerProvider
    {

        services.AddSingleton(provider =>
        {
            var instanceFactory = new Func<object>(() => provider.GetRequiredService<THandler>());
            return new HandlerDescriptor(instanceFactory, typeof(THandler), streamName);
        });

        return services;
    }
    public static IServiceCollection AddSingletonHandler<THandler>(this IServiceCollection services, string? streamName = null)
        where THandler : class, IHandlerProvider
    {

        services.AddSingleton(provider =>
        {
            var instance = provider.GetRequiredService<THandler>();
            var instanceFactory = () => provider.GetRequiredService<THandler>();
            return new HandlerDescriptor(instanceFactory, typeof(THandler), streamName);
        });

        return services;
    }
}
