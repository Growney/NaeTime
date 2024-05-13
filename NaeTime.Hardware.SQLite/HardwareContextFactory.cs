using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Hardware.SQLite;
internal class HardwareContextFactory : IDesignTimeDbContextFactory<HardwareDbContext>
{
    public HardwareDbContext CreateDbContext(string[] args)
    {
        string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.hardware.db");

        DbContextOptionsBuilder<HardwareDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new HardwareDbContext(optionsBuilder.Options);
    }
}