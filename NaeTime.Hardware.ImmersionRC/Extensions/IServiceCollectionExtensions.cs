using ImmersionRC.LapRF.Extensions;
using NaeTime.Hardware.ImmersionRC;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Timing.ImmersionRC;
using NaeTime.Timing.ImmersionRC.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddImmersionRCHardware(this IServiceCollection services)
    {
        services.AddSingleton<LapRFManager>();
        services.AddSingleton<ILapRFManager>(x => x.GetRequiredService<LapRFManager>());
        services.AddHostedService(x => x.GetRequiredService<LapRFManager>());
        services.AddTransient<ILapRFConnectionFactory, LapRFConnectionFactory>();
        services.AddEventHub<LapRFLaneManager>();
        services.AddImmersionRCLapRF();
        return services;
    }
}
