using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Client.Configuration;
using NaeTime.Client.Configuration.Abstractions;

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
