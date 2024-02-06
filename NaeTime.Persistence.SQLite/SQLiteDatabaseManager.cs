using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
internal class SQLiteDatabaseManager : IHostedService
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLiteDatabaseManager(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task StartAsync(CancellationToken cancellationToken) => _dbContext.Database.MigrateAsync();


    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
