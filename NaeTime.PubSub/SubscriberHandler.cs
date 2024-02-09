using NaeTime.PubSub.Abstractions;
using System.Reflection;

namespace NaeTime.PubSub;
internal class SubscriberHandler : ISubscriptionHandler
{
    private readonly Type _messageType;
    private readonly Func<object> _subscriber;
    private readonly MethodInfo _whenMethodInfo;

    public Type SubscriberType { get; }

    public SubscriberHandler(Type subscriberType, Type messageType, Func<object> subscriber)
    {
        var method = FindMethodInfo(subscriberType, messageType);

        if (method == null || method.ReturnType != typeof(Task))
        {
            throw new ArgumentException($"Subscriber type does not have a When method for {messageType}");
        }

        _whenMethodInfo = method;
        _subscriber = subscriber;
        _messageType = messageType;
        SubscriberType = subscriberType;
    }
    public async Task Handle(object message)
    {
        if (message.GetType() != _messageType)
        {
            throw new ArgumentException($"Message type does not match subscriber type. Expected {_messageType} but got {message.GetType()}");
        }

        var subscriber = _subscriber();
        var result = _whenMethodInfo.Invoke(subscriber, [message]);

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
