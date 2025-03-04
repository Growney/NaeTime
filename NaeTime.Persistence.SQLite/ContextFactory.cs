using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Persistence.EntityFramework;
internal class ContextFactory : IDesignTimeDbContextFactory<NaeTimeDbContext>
{
    public NaeTimeDbContext CreateDbContext(string[] args)
    {
        string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.db");

        DbContextOptionsBuilder<NaeTimeDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}", x => x.MigrationsAssembly("NaeTime.Persistence.SQLite"));

        return new NaeTimeDbContext(optionsBuilder.Options);
    }
}