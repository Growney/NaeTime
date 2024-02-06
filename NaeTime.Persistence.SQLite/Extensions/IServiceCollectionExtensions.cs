using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.SQLite;
using NaeTime.Persistence.SQLite.Context;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSQLitePersistence(this IServiceCollection services)
    {
        services.AddHostedService<SQLiteDatabaseManager>();
        services.AddDbContext<NaeTimeDbContext>(options =>
        {
            options.UseSqlite($"Data Source=naetime.db");
        });

        return services;
    }
}
