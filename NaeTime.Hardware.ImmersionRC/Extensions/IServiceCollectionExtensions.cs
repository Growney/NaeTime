using ImmersionRC.LapRF.Extensions;
using NaeTime.Timing.ImmersionRC;
using NaeTime.Timing.ImmersionRC.Abstractions;
using NaeTime.Timing.ImmersionRC.Hardware;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddImmersionRCHardware(this IServiceCollection services)
    {
        services.AddHostedService<LapRFManager>();
        services.AddSubscriberAssembly(typeof(LapRFManager).Assembly);
        services.AddTransient<ILapRFConnectionFactory, LapRFConnectionFactory>();
        services.AddImmersionRCLapRF();
        return services;
    }
}
