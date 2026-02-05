using EventDb.Sqlite;
using EventDb.Sqlite.Abstractions;
using EventDb.Sqlite.Connections;
using EventDbLite;
using EventDbLite.Abstractions;
using EventDbLite.Aggregates;
using EventDbLite.Handlers;
using EventDbLite.Handlers.Abstractions;
using EventDbLite.Projections;
using EventDbLite.Reactions;
using EventDbLite.Reactions.Abstractions;
using EventDbLite.Serialization;
using EventDbLite.Streams;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.SQLite;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddSQLiteEventDbLite(this IServiceCollection services)
    {
        services.AddDbContext<EventDbLiteContext>(options =>
        {
            SqliteConnectionStringBuilder builder = new()
            {
                DataSource = "eventdblite.db",
                Cache = SqliteCacheMode.Private,
                Pooling = false,
            };

            string connectionString = builder.ToString();

            options.UseSqlite(connectionString)
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors();
        });
        services.AddEventDbLite();
        services.AddTransient<ISqliteConnectionFactory, SqliteConnectionFactory>();

        services.AddHostedService<SQLiteDatabaseManager<EventDbLiteContext>>();
        services.AddSingleton<IEventStreamConnection, EventStreamConnection>();
        return services;
    }
}
