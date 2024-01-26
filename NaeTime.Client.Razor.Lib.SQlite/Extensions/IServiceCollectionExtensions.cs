using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.SQlite;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddLocalDbLocalClientProvider<TStorage>(this IServiceCollection services)
        where TStorage : class, ISimpleStorageProvider
    {
        services.AddLocalApiClientProvider<LocalSqliteApiClientProvider, LocalDbApiConfiguration, TStorage>();

        return services;
    }
}
