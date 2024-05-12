using ImmersionRC.LapRF.Extensions;
using NaeTime.Hardware.ImmersionRC;
using NaeTime.Timing.ImmersionRC;
using NaeTime.Timing.ImmersionRC.Abstractions;
using NaeTime.Timing.ImmersionRC.Hardware;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddImmersionRCHardware(this IServiceCollection services)
    {
        services.AddHostedService<LapRFManager>();
        services.AddTransient<ILapRFConnectionFactory, LapRFConnectionFactory>();
        services.AddEventAndRemoteProcedureCallHub<EthernetLapRF8ChannelTimerLaneService>();
        services.AddImmersionRCLapRF();
        return services;
    }
}
