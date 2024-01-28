using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.PubSub;
public class UniversalPublisher : IUniversalPublisher, IPublisher
{
    private ConcurrentDictionary<object, ConcurrentDictionary<Type, int>> _instanceTypes = new();
    private ConcurrentDictionary<Type, ConcurrentDictionary<object, Func<object, Task>>> _subscriptions = new();

    public Task Publish(object message)
    {
        var messageType = message.GetType();

        if (!_subscriptions.TryGetValue(messageType, out var typeSubscriptions))
        {
            return Task.CompletedTask;
        }

        var tasks = new List<Task>();

        foreach (var subscription in typeSubscriptions)
        {
            tasks.Add(subscription.Value(message));
        }

        return Task.WhenAll(tasks);
    }


    public void Subscribe<TMessageType>(object subscriber, Func<TMessageType, Task> onMessage)
    {
        var typeSubscriptions = _subscriptions.GetOrAdd(typeof(TMessageType), _ => new ConcurrentDictionary<object, Func<object, Task>>());
        typeSubscriptions.TryAdd(subscriber, (message) => onMessage((TMessageType)message));

        var instanceSubscribedTypes = _instanceTypes.GetOrAdd(subscriber, _ => new ConcurrentDictionary<Type, int>());
        instanceSubscribedTypes.TryAdd(typeof(TMessageType), 0);
    }

    public void Unsubscribe<TMessageType>(object subscriber)
    {
        var messageType = typeof(TMessageType);

        if (!_subscriptions.TryGetValue(messageType, out var typeSubscriptions))
        {
            return;
        }

        typeSubscriptions.TryRemove(subscriber, out _);

        if (!_instanceTypes.TryGetValue(subscriber, out var instanceSubscribedTypes))
        {
            return;
        }

        instanceSubscribedTypes.TryRemove(messageType, out _);


        if (instanceSubscribedTypes.Count == 0)
        {
            _instanceTypes.TryRemove(subscriber, out _);
        }

    }

    public void Unsubscribe(object subscriber)
    {
        if (!_instanceTypes.TryGetValue(subscriber, out var instanceSubscribedTypes))
        {
            return;
        }

        foreach (var typeKvp in instanceSubscribedTypes)
        {
            if (!_subscriptions.TryGetValue(typeKvp.Key, out var typeSubscriptions))
            {
                continue;
            }

            typeSubscriptions.TryRemove(subscriber, out _);
        }
        _instanceTypes.TryRemove(subscriber, out _);
    }
}
