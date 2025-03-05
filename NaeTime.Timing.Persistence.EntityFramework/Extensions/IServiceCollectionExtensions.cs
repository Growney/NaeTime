using NaeTime.Timing.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkTiming(this IServiceCollection services)
    {
        services.AddEventAndRemoteProcedureCallHub<LaneService>();

        return services;
    }
}
