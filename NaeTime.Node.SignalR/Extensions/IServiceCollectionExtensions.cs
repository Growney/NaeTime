using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.SignalR;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalRNode(this IServiceCollection services)
    {
        services.AddTransient<INodeClientFactory, SignalRNodeClientFactory>();

        return services;
    }
}
