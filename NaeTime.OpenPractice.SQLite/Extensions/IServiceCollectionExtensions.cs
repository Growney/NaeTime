using NaeTime.OpenPractice.SQLite;
using NaeTime.Persistence.SQLite;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSQLiteOpenPractice(this IServiceCollection services)
    {
        services.AddSubscriberAssembly(typeof(OpenPracticeDbContext).Assembly);
        services.AddHostedService<SQLiteDatabaseManager<OpenPracticeDbContext>>();
        services.AddDbContext<OpenPracticeDbContext>(options => options.UseSqlite($"Data Source=naetime.openpractice.db"), contextLifetime: ServiceLifetime.Transient);

        return services;
    }
}
