using NaeTime.OpenPractice.SQLite;
using NaeTime.Persistence.SQLite;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSQLiteTiming(this IServiceCollection services)
    {
        services.AddEventAndRemoteProcedureCallHub<LaneService>();
        services.AddEventAndRemoteProcedureCallHub<ActiveTimingService>();

        services.AddHostedService<SQLiteDatabaseManager<TimingDbContext>>();
        services.AddDbContext<TimingDbContext>(options => options.UseSqlite($"Data Source=naetime.timing.db"), contextLifetime: ServiceLifetime.Transient);
        return services;
    }
}
