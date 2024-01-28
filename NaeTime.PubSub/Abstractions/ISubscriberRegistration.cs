using Microsoft.Extensions.DependencyInjection;

namespace NaeTime.PubSub.Abstractions;
public interface ISubscriberRegistration
{
    Type SubscriberType { get; }
    Type MessageType { get; }
    ServiceLifetime Lifetime { get; }
}
