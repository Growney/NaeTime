using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NaeTime.Persistence.SQLite;
public class SQLiteDatabaseManager<T>(IServiceProvider serviceProvider) : IHostedService
    where T : DbContext
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IServiceScope scope = _serviceProvider.CreateScope(); ;
        try
        {
            T dbContext = scope.ServiceProvider.GetRequiredService<T>();
            await dbContext.Database.MigrateAsync();
        }
        finally
        {
            scope.Dispose();
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
