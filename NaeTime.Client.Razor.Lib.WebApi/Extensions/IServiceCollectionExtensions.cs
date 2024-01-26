using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.WebApi;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceColectionExtensionMethods
{
    public static IServiceCollection AddLocalWebApiClientProvider<TStorage>(this IServiceCollection services)
        where TStorage : class, ISimpleStorageProvider
    {

        services.AddHttpClient();

        services.AddLocalApiClientProvider<LocalWebApiClientProvider, LocalWebApiConfiguration, TStorage>();


        return services;
    }

    public static IServiceCollection AddOffSiteAPIClientProvider<TStorage>(this IServiceCollection services)
        where TStorage : class, ISimpleStorageProvider
    {
        services.AddHttpClient();
        services.TryAddSingleton<ISimpleStorageProvider, TStorage>();

        services.AddSingleton<OffSiteWebApiConfiguration>();
        services.AddSingleton<IOffSiteApiConfiguration>(x => x.GetRequiredService<OffSiteWebApiConfiguration>());
        services.AddScoped<IOffSiteApiClientProvider, OffSiteWebApiClientProvider>();

        return services;
    }
}
