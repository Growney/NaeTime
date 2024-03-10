using Microsoft.EntityFrameworkCore.Design;
using NaeTime.OpenPractice.SQLite;

namespace NaeTime.Persistence.SQLite.Context;
internal class OpenPracticeContextFactory : IDesignTimeDbContextFactory<OpenPracticeDbContext>
{
    public OpenPracticeDbContext CreateDbContext(string[] args)
    {
        var appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.openpractice.db");

        var optionsBuilder = new DbContextOptionsBuilder<OpenPracticeDbContext>();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new OpenPracticeDbContext(optionsBuilder.Options);
    }
}