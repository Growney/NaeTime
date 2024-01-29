using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimePublishSubscribe(this IServiceCollection services)
    {
        services.TryAddSingleton<PublishSubscribe>();
        services.TryAddSingleton<IPublishSubscribe>(x => x.GetRequiredService<PublishSubscribe>());
        services.TryAddSingleton<IDispatcher>(x => x.GetRequiredService<PublishSubscribe>());
        services.TryAddSingleton<IPublisher>(x => x.GetRequiredService<PublishSubscribe>());

        return services;
    }

    public static IServiceCollection AddSubscriber<TSubscriber, TMessage>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Transient)
        where TSubscriber : ISubscriber
    {
        services.AddSingleton<ISubscriberRegistration>(new SubscriberRegistration(typeof(TSubscriber), typeof(TMessage), lifeTime));
        return services;
    }
    public static IServiceCollection AddSubscriber<TSubscriber, TRequest, TResponse>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Transient)
        where TSubscriber : ISubscriber
    {
        services.AddSingleton<IHandlerRegistration>(new HandlerRegistration(typeof(TSubscriber), typeof(TRequest), typeof(TResponse), lifeTime));
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
        var messageTypes = GetMessageTypes(subscriberType);
        foreach (var messageType in messageTypes)
        {
            services.AddSingleton<ISubscriberRegistration>(new SubscriberRegistration(subscriberType, messageType, ServiceLifetime.Transient));
        }
        var handlerTypes = GetHandlerTypes(subscriberType);
        foreach (var handlerType in handlerTypes)
        {
            services.AddSingleton<IHandlerRegistration>(new HandlerRegistration(subscriberType, handlerType.requestType, handlerType.responseType, ServiceLifetime.Transient));
        }

        return services;
    }

    private static IEnumerable<(Type requestType, Type responseType)> GetHandlerTypes(Type handlerType)
    {
        var methods = handlerType.GetMethods();

        foreach (var method in methods)
        {
            if (method.Name != "On")
            {
                continue;
            }
            var parameters = method.GetParameters();

            if (parameters.Length != 1)
            {
                continue;
            }

            var requestType = parameters[0].ParameterType;

            var returnType = method.ReturnType;

            Type responseType;
            if (returnType.IsGenericType)
            {
                if (returnType.GetGenericTypeDefinition() != typeof(Task<>))
                {
                    continue;
                }
                responseType = returnType.GetGenericArguments().First();
            }
            else
            {
                responseType = returnType;
            }

            yield return (requestType, responseType);

        }
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
