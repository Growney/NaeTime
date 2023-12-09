using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.WebApi;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceColectionExtensionMethods
{
    public static IServiceCollection AddLocalWebApiClientProvider<TStorage>(this IServiceCollection services)
        where TStorage : class, ISimpleStorageProvider
    {
        services.AddSingleton<ISimpleStorageProvider, TStorage>();

        services.AddSingleton<LocalWebApiConfiguration>();
        services.AddSingleton<ILocalApiConfiguration>(x => x.GetRequiredService<LocalWebApiConfiguration>());
        services.AddScoped<ILocalApiClientProvider, LocalWebApiClientProvider>();

        services.AddSingleton<OffsiteWebApiConfiguration>();
        services.AddSingleton<IOffSiteApiConfiguration>(x => x.GetRequiredService<OffsiteWebApiConfiguration>());
        services.AddScoped<IOffSiteApiClientProvider, OffSiteWebApiClientProvider>();

        return services;
    }
}
