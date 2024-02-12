using Microsoft.Extensions.DependencyInjection;
using NaeTime.PubSub.Abstractions;
using System.Reflection;

namespace NaeTime.PubSub;
internal class SubscriberHandler : ISubscriptionHandler
{
    private readonly Type _messageType;
    private readonly MethodInfo _whenMethodInfo;
    private readonly IServiceProvider _serviceProvider;
    public ServiceLifetime SubscriberLifetime { get; }
    public Type SubscriberType { get; }
    private object? _subscriber;

    public SubscriberHandler(IServiceProvider serviceProvider, ServiceLifetime subscriberLifetime, Type subscriberType, Type messageType)
    {
        var method = FindMethodInfo(subscriberType, messageType);

        if (method == null || method.ReturnType != typeof(Task))
        {
            throw new ArgumentException($"Subscriber type does not have a When method for {messageType}");
        }

        SubscriberLifetime = subscriberLifetime;
        _whenMethodInfo = method;
        _messageType = messageType;
        _serviceProvider = serviceProvider;
        SubscriberType = subscriberType;
    }
    public async Task Handle(object message)
    {
        if (message.GetType() != _messageType)
        {
            throw new ArgumentException($"Message type does not match subscriber type. Expected {_messageType} but got {message.GetType()}");
        }

        switch (SubscriberLifetime)
        {
            case ServiceLifetime.Singleton:
                await HandleScopedMessage(message);
                break;
            case ServiceLifetime.Scoped:
                await HandleScopedMessage(message);
                break;
            case ServiceLifetime.Transient:
                await HandleTransientMessage(message);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    private Task HandleScopedMessage(object message)
    {
        using var scope = _serviceProvider.CreateScope();

        var subscriber = ActivatorUtilities.CreateInstance(scope.ServiceProvider, SubscriberType);

        var result = _whenMethodInfo.Invoke(subscriber, [message]);

        return AwaitResult(result);
    }
    private Task HandleTransientMessage(object message)
    {
        var subscriber = ActivatorUtilities.CreateInstance(_serviceProvider, SubscriberType);

        var result = _whenMethodInfo.Invoke(subscriber, [message]);

        return AwaitResult(result);
    }
    private Task HandleSingletonMessage(object message)
    {
        var subscriber = _subscriber ?? ActivatorUtilities.CreateInstance(_serviceProvider, SubscriberType);

        var result = _whenMethodInfo.Invoke(subscriber, [message]);

        return AwaitResult(result);
    }
    private async Task AwaitResult(object? result)
    {
        if (result is Task task)
        {
            await task;
        }
        else if (result is ValueTask valueTask)
        {
            await valueTask;
        }
    }
    private MethodInfo? FindMethodInfo(Type subscriberType, Type messageType)
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

            if (parameter.ParameterType != messageType)
            {
                continue;
            }

            return method;

        }

        return null;
    }
}
