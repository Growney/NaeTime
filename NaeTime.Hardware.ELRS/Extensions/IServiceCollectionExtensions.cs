using NaeTime.Hardware.ELRS;
using NaeTime.Hardware.ELRS.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddBackpack(this IServiceCollection services)
    {
        services.AddSingleton<IBackpackConnectorProvider, BackpackConnectorProvider>();
        services.AddHostedService<BackpackManager>();
        services.AddELRSBackpack();

        services.AddConstantReactionClass<BackpackReactions>();

        return services;
    }
}
