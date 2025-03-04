using NaeTime.Hardware.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkHardware(this IServiceCollection services)
    {
        services.AddEventAndRemoteProcedureCallHub<HardwareService>();
        services.AddEventAndRemoteProcedureCallHub<DetectionService>();
        return services;
    }
}
