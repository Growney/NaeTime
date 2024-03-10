using NaeTime.Timing;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTimingCore(this IServiceCollection services)
    {
        services.AddSubscriberAssembly(typeof(LapService).Assembly);
        return services;
    }
}
