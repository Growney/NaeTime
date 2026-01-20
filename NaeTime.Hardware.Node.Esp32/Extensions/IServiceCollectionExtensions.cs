using NaeTime.Hardware.Node.Esp32;
using NaeTime.Hardware.Node.Esp32.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEsp32NodeTimers(this IServiceCollection services)
    {
        services.AddHostedService<NodeManager>();
        services.AddTransient<INodeConnectionFactory, NodeConnectionFactory>();
        services.AddSingleton<INodeConnectionProvider, NodeConnectionProvider>();

        services.AddConstantReactionClass<NaeTimeNodeReactions>();
        return services;
    }
}
