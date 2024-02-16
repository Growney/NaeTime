using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Persistence.Client;
using NaeTime.Persistence.Client.Abstractions;
using NaeTime.Persistence.Extensions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddClientPersistence<TSimpleStorage>(this IServiceCollection services)
        where TSimpleStorage : class, ISimpleStorageProvider
    {
        services.TryAddTransient<ISimpleStorageProvider, TSimpleStorage>();
        services.TryAddTransient<ILocalConfigurationRepository, LocalConfigurationRepository>();
        services.AddSubscriberAssembly(typeof(ClientMode).Assembly);
        services.AddPersistence<ClientRepositoryFactory>();
        services.AddSQLitePersistence();
        return services;
    }
}
