using Microsoft.Extensions.DependencyInjection;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.PubSub;
public class SubscriberRegistration : ISubscriberRegistration
{
    public SubscriberRegistration(Type subscriberType, Type messageType, ServiceLifetime lifetime)
    {
        SubscriberType = subscriberType ?? throw new ArgumentNullException(nameof(subscriberType));
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        Lifetime = lifetime;
    }

    public Type SubscriberType { get; }
    public Type MessageType { get; }
    public ServiceLifetime Lifetime { get; }

}
