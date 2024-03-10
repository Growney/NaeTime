using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace NaeTime.Persistence.SQLite;
public class SQLiteDatabaseManager<T> : IHostedService
    where T : DbContext
{
    private readonly T _dbContext;

    public SQLiteDatabaseManager(T dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task StartAsync(CancellationToken cancellationToken) => _dbContext.Database.MigrateAsync();
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
