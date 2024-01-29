using Microsoft.Extensions.DependencyInjection;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.PubSub;
internal class HandlerRegistration : IHandlerRegistration
{
    public HandlerRegistration(Type handlerType, Type requestType, Type responseType, ServiceLifetime lifetime)
    {
        HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
        ResponseType = responseType ?? throw new ArgumentNullException(nameof(responseType));
        Lifetime = lifetime;
    }

    public Type HandlerType { get; }

    public Type RequestType { get; }

    public Type ResponseType { get; }

    public ServiceLifetime Lifetime { get; }
}
