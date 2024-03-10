using NaeTime.Persistence.SQLite;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSQLiteManagement(this IServiceCollection services)
    {
        services.AddSubscriberAssembly(typeof(ManagementDbContext).Assembly);
        services.AddHostedService<SQLiteDatabaseManager<ManagementDbContext>>();
        services.AddDbContext<ManagementDbContext>(options => options.UseSqlite($"Data Source=naetime.management.db"), contextLifetime: ServiceLifetime.Transient);
        return services;
    }
}
