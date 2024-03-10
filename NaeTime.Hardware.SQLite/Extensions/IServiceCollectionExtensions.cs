using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.SQLite;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSQLiteHardware(this IServiceCollection services)
    {
        services.AddSubscriberAssembly(typeof(HardwareDbContext).Assembly);
        services.AddHostedService<SQLiteDatabaseManager<HardwareDbContext>>();
        services.AddDbContext<HardwareDbContext>(options => options.UseSqlite($"Data Source=naetime.hardware.db"), contextLifetime: ServiceLifetime.Transient);

        return services;
    }
}
