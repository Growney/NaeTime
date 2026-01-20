using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Hardware;
using NaeTime.Hardware.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddHardwareCore(this IServiceCollection services)
    {
        services.TryAddSingleton<ISoftwareTimer, SoftwareTimer>();
        services.AddHostedService<RssiDistributionService>();
        services.AddSingleton<RssiChannel>();
        services.AddSingleton<IRssiChannel>(s=> s.GetRequiredService<RssiChannel>());
        return services;
    }
}
