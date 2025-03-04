using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Persistence.SQLite.Extensions;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSQLiteDbContext(this IServiceCollection services)
    {
        string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NaeTime");

        string dbPath = Path.Combine(appDirectory, "naetime.db");

        if (!Directory.Exists(appDirectory))
        {
            Directory.CreateDirectory(appDirectory);
        }

        services.AddHostedService<SQLiteDatabaseManager<NaeTimeDbContext>>();
        services.AddDbContext<NaeTimeDbContext>(options => options.UseSqlite($"Data Source={dbPath}", x => x.MigrationsAssembly("NaeTime.Persistence.SQLite")), contextLifetime: ServiceLifetime.Transient);

        return services;
    }
}
