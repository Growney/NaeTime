using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Persistence.SQLite.Context;
internal class ManagementContextFactory : IDesignTimeDbContextFactory<ManagementDbContext>
{
    public ManagementDbContext CreateDbContext(string[] args)
    {
        string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.management.db");

        DbContextOptionsBuilder<ManagementDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new ManagementDbContext(optionsBuilder.Options);
    }
}