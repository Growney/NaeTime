using Microsoft.Extensions.DependencyInjection;
using NaeTime.Hardware.Node.Esp32.Abstractions;

namespace NaeTime.Hardware.Node.Esp32.Extensions;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEsp32NodeTimers(this IServiceCollection services)
    {
        services.AddSingleton<NodeManager>();
        services.AddSingleton<INodeManager>(x => x.GetRequiredService<NodeManager>());
        services.AddHostedService<NodeManager>(x => x.GetRequiredService<NodeManager>());
        services.AddTransient<INodeConnectionFactory, NodeConnectionFactory>();
        return services;
    }
}
