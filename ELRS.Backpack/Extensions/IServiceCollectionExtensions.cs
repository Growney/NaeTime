using ELRS.Backpack;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddELRSBackpack(this IServiceCollection services)
    {
        services.AddSingleton<IBackpackConnectionFactory, BackpackConnectionFactory>();
        services.AddSingleton<IBackpackCommandSerializer, BackpackCommandSerializer>();
        return services;
    }
}
