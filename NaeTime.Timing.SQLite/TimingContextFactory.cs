using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Persistence.SQLite.Context;
internal class TimingContextFactory : IDesignTimeDbContextFactory<TimingDbContext>
{
    public TimingDbContext CreateDbContext(string[] args)
    {
        string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.timing.db");

        DbContextOptionsBuilder<TimingDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new TimingDbContext(optionsBuilder.Options);
    }
}