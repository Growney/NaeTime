using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Domain;
using NaeTime.Node.Services;

namespace NaeTime.Node.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNodeManager(this IServiceCollection services)
    {
        services.AddSingleton<NodeManager>();
        services.AddHostedService(x => x.GetRequiredService<NodeManager>());
        services.TryAddTransient<INodeConfigurationManager, NodeConfigurationManager>();
        services.TryAddSingleton<INodeDeviceFactory, NodeDeviceFactory>();

        return services;
    }
}
