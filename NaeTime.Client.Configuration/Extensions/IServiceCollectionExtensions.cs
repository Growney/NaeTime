using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Persistence.Client;
using NaeTime.Persistence.Client.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddLocalClientConfiguration<TSimpleStorage>(this IServiceCollection services)
        where TSimpleStorage : class, ISimpleStorageProvider
    {

        services.TryAddSingleton<ISimpleStorageProvider, TSimpleStorage>();
        services.TryAddSingleton<ILocalConfigurationRepository, SimpleStorageConfigurationRepository>();

        return services;
    }
}
