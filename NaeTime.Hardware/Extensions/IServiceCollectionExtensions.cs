using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Hardware.Abstractions;
using NaeTime.Timing;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddHardwareCore(this IServiceCollection services)
    {
        services.TryAddSingleton<ISoftwareTimer, SoftwareTimer>();

        services.AddSubscriberAssembly(typeof(ISoftwareTimer).Assembly);

        return services;
    }
}
