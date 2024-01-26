using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Client.Razor.Lib.SQlite;
public class NaeTimeContextFactory : IDesignTimeDbContextFactory<NaeTimeDbContext>
{
    public NaeTimeDbContext CreateDbContext(string[] args)
    {
        var appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.db");

        var optionsBuilder = new DbContextOptionsBuilder<NaeTimeDbContext>();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new NaeTimeDbContext(optionsBuilder.Options);
    }
}
