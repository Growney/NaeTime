using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NaeTime.Hardware.SQLite;
internal class HardwareContextFactory : IDesignTimeDbContextFactory<HardwareDbContext>
{
    public HardwareDbContext CreateDbContext(string[] args)
    {
        var appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "naetime.hardware.db");

        var optionsBuilder = new DbContextOptionsBuilder<HardwareDbContext>();
        optionsBuilder.UseSqlite($"Data Source={appDirectory}");

        return new HardwareDbContext(optionsBuilder.Options);
    }
}