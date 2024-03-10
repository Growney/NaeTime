using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Persistence.SQLite.Context;
internal class TimingContextFactory : IDesignTimeDbContextFactory<TimingDbContext>
{
    public TimingDbContext CreateDbContext(string[] args)
    {
        var appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.timing.db");

        var optionsBuilder = new DbContextOptionsBuilder<TimingDbContext>();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new TimingDbContext(optionsBuilder.Options);
    }
}