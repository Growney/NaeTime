using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimePublishSubscribe(this IServiceCollection services)
    {
        services.TryAddSingleton<UniversalPublisher>();
        services.TryAddSingleton<IUniversalPublisher>(x => x.GetRequiredService<UniversalPublisher>());
        services.TryAddSingleton<IPublisher>(x => x.GetRequiredService<UniversalPublisher>());
        services.TryAddSingleton<IDispatcher, Dispatcher>();

        return services;
    }

    public static IServiceCollection AddSubscriber<TSubscriber, TMessage>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Transient)
        where TSubscriber : ISubscriber
    {
        services.AddSingleton<ISubscriberRegistration>(new SubscriberRegistration(typeof(TSubscriber), typeof(TMessage), lifeTime));
        return services;
    }
    public static IServiceCollection AddSubscriber(this IServiceCollection services, Type subscriberType)
    {
        if (!subscriberType.IsClass)
        {
            return services;
        }

        if (subscriberType.IsAbstract)
        {
            return services;
        }
        if (!subscriberType.IsAssignableTo(typeof(ISubscriber)))
        {
            return services;
        }
        ServiceLifetime lifetime;
        if (subscriberType.IsAssignableTo(typeof(ISingletonSubscriber)))
        {
            lifetime = ServiceLifetime.Singleton;
        }
        else
        {
            lifetime = ServiceLifetime.Transient;
        }
        var messageTypes = GetMessageTypes(subscriberType);
        foreach (var messageType in messageTypes)
        {
            services.AddSingleton<ISubscriberRegistration>(new SubscriberRegistration(subscriberType, messageType, lifetime));
        }

        return services;
    }

    private static IEnumerable<Type> GetMessageTypes(Type subscriberType)
    {
        var methods = subscriberType.GetMethods();

        foreach (var method in methods)
        {
            if (method.Name != "When")
            {
                continue;
            }
            var parameters = method.GetParameters();

            if (parameters.Length != 1)
            {
                continue;
            }

            var parameter = parameters[0];

            yield return parameter.ParameterType;
        }
    }


    public static IServiceCollection AddSubscriberAssembly(this IServiceCollection services, Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();

        foreach (var potentialSubscriberType in assemblyTypes)
        {
            services.AddSubscriber(potentialSubscriberType);
        }

        return services;
    }
}
