using Microsoft.EntityFrameworkCore.Design;
using NaeTime.OpenPractice.SQLite;

namespace NaeTime.Persistence.SQLite.Context;
internal class OpenPracticeContextFactory : IDesignTimeDbContextFactory<OpenPracticeDbContext>
{
    public OpenPracticeDbContext CreateDbContext(string[] args)
    {
        string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.openpractice.db");

        DbContextOptionsBuilder<OpenPracticeDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new OpenPracticeDbContext(optionsBuilder.Options);
    }
}