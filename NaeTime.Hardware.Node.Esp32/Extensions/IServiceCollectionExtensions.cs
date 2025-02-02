using Microsoft.Extensions.DependencyInjection;
using NaeTime.Hardware.Node.Esp32.Abstractions;

namespace NaeTime.Hardware.Node.Esp32.Extensions;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEsp32NodeTimers(this IServiceCollection services)
    {
        services.AddHostedService<NodeManager>();
        services.AddTransient<INodeConnectionFactory, NodeConnectionFactory>();
        services.AddEventAndRemoteProcedureCallHub<NodeTimerLaneService>();
        return services;
    }
}
