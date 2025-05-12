using NaeTime.Hardware.ELRS;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddBackpack(this IServiceCollection services)
    {
        services.AddHostedService<BackpackManager>();
        services.AddELRSBackpack();
        return services;
    }
}
