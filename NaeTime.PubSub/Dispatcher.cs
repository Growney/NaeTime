using Microsoft.Extensions.DependencyInjection;
using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.PubSub;
internal class Dispatcher : IDispatcher
{
    private readonly IUniversalPublisher _publisher;
    private readonly IServiceProvider _serviceProvider;

    private ConcurrentDictionary<Type, IEnumerable<SubscriberHandler>> _subscribers = new();

    public Dispatcher(IUniversalPublisher publisher, IServiceProvider serviceProvider)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task Dispatch(object message)
    {
        var publishTasks = new List<Task>
        {
            _publisher.Publish(message)
        };

        var subscriberBag = _subscribers.GetOrAdd(message.GetType(), _ =>
        {
            var subscribers = _serviceProvider.GetServices<ISubscriberRegistration>();

            var bag = new List<SubscriberHandler>();
            var messageType = message.GetType();

            foreach (var registration in subscribers)
            {
                if (registration.MessageType != messageType)
                {
                    continue;
                }

                Func<object> subscriber;
                if (registration.Lifetime == ServiceLifetime.Singleton)
                {
                    var instance = ActivatorUtilities.CreateInstance(_serviceProvider, registration.SubscriberType);
                    subscriber = () => instance;
                }
                else
                {
                    subscriber = () => ActivatorUtilities.CreateInstance(_serviceProvider, registration.SubscriberType);
                }

                var handler = new SubscriberHandler(registration.SubscriberType, registration.MessageType, subscriber);

                bag.Add(handler);
            }

            return bag;
        });

        foreach (var subscriber in subscriberBag)
        {
            publishTasks.Add(subscriber.Handle(message));
        }

        await Task.WhenAll(publishTasks);
    }
}
