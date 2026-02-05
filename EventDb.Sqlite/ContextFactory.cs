using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventDb.Sqlite;
internal class ContextFactory : IDesignTimeDbContextFactory<EventDbLiteContext>
{
    public EventDbLiteContext CreateDbContext(string[] args)
    {
        string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "eventdblite.db");

        DbContextOptionsBuilder<EventDbLiteContext> optionsBuilder = new();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}", x => x.MigrationsAssembly("EventDbLite"));

        return new EventDbLiteContext(optionsBuilder.Options);
    }
}