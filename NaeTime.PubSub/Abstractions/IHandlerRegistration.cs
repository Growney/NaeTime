using Microsoft.Extensions.DependencyInjection;

namespace NaeTime.PubSub.Abstractions;
internal interface IHandlerRegistration
{
    Type HandlerType { get; }
    Type RequestType { get; }
    Type ResponseType { get; }
    ServiceLifetime Lifetime { get; }
}
