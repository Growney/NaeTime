using EventDbLite.Abstractions;
using EventDbLite.KurrentDb;
using KurrentDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{

   public static IServiceCollection AddKurrentDbEventDbLite(this IServiceCollection services, Action<KurrentDbClientSettingsBuilder>? configure)
    {
        services.TryAddSingleton(serviceProvider =>
        {
            IConfiguration? configuration = serviceProvider.GetService<IConfiguration>();

            string? connectionString = configuration?.GetConnectionString("KurrentDB");

            KurrentDBClientSettings defaultSettings;
            if (string.IsNullOrEmpty(connectionString))
            {
                defaultSettings = new();
            }
            else
            {
                defaultSettings = KurrentDBClientSettings.Create(connectionString);
            }

            KurrentDbClientSettingsBuilder builder = new(serviceProvider, defaultSettings);

            configure?.Invoke(builder);

            return new KurrentDBClient(builder.Settings);
        });
        services.AddEventDbLite();
        services.AddSingleton<IEventStoreLite, KurrentDbEventStoreLite>();
        return services;
    }
}
