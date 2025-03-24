using NaeTime.Hardware.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkHardware(this IServiceCollection services)
    {
        services.AddEventHub<HardwareService>();
        services.AddEventHub<DetectionService>();
        return services;
    }
}
