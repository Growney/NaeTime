using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Timing;
using NaeTime.Timing.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTimingCore(this IServiceCollection services)
    {
        services.TryAddSingleton<ISoftwareTimer, SoftwareTimer>();
        services.AddSubscriberAssembly(typeof(SoftwareTimer).Assembly);
        services.AddTransient<ISessionFactory, SessionFactory>();
        return services;
    }
}
