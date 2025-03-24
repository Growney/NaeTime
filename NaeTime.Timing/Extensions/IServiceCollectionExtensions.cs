using NaeTime.Timing;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTimingCore(this IServiceCollection services)
    {
        services.AddEventHub<SessionDetectionService>();
        services.AddEventHub<LapService>();
        return services;
    }

}
