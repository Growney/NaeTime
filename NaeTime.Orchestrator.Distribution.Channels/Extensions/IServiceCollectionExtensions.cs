using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Management;
using NaeTime.Orchestrator.Distribution.Channels;
using System.Threading.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddChannelsOrchestratorDistribution(this IServiceCollection services)
    {
        services.AddScoped<IDistributionReceiver, ChannelsDistributionReceiver>();

        services.AddSingleton(x => Channel.CreateUnbounded<OpenPracticeSessionActivated>());
        services.AddSingleton(x => Channel.CreateUnbounded<SessionDeactivated>());
        services.AddSingleton(x => Channel.CreateUnbounded<TimerConnected>());
        services.AddSingleton(x => Channel.CreateUnbounded<TimerDisconnected>());

        services.AddScoped<INaeTimeOrchestratorDistribution, ChannelsNaeTimeOrchestratorDistribution>();

        return services;
    }
}
