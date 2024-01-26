using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddLocalApiClientProvider<TProviderType, TConfigurationType, TStorage>(this IServiceCollection services)
        where TProviderType : class, ILocalApiClientProvider
        where TConfigurationType : class, ILocalApiConfiguration
        where TStorage : class, ISimpleStorageProvider
    {
        services.AddSingleton<TConfigurationType>();
        services.AddSingleton<ILocalApiConfiguration>(x => x.GetRequiredService<TConfigurationType>());
        services.TryAddSingleton<ISimpleStorageProvider, TStorage>();
        services.AddScoped<ILocalApiClientProvider, TProviderType>();
        services.AddScoped(x => x.GetRequiredService<ILocalApiClientProvider>().HardwareApiClient);
        services.AddScoped(x => x.GetRequiredService<ILocalApiClientProvider>().PilotApiClient);
        services.AddScoped(x => x.GetRequiredService<ILocalApiClientProvider>().FlyingSessionApiClient);
        services.AddScoped(x => x.GetRequiredService<ILocalApiClientProvider>().TrackApiClient);

        return services;
    }
}
