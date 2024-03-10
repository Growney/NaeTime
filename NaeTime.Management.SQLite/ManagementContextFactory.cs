using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Persistence.SQLite.Context;
internal class ManagementContextFactory : IDesignTimeDbContextFactory<ManagementDbContext>
{
    public ManagementDbContext CreateDbContext(string[] args)
    {
        var appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.management.db");

        var optionsBuilder = new DbContextOptionsBuilder<ManagementDbContext>();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new ManagementDbContext(optionsBuilder.Options);
    }
}