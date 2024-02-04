using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Persistence.SQLite.Context;
public class NateTimeContextFactory : IDesignTimeDbContextFactory<NaeTimeDbContext>
{
    public NaeTimeDbContext CreateDbContext(string[] args)
    {
        var appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.db");

        var optionsBuilder = new DbContextOptionsBuilder<NaeTimeDbContext>();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new NaeTimeDbContext(optionsBuilder.Options);
    }
}