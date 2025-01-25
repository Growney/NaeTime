using Microsoft.EntityFrameworkCore;

namespace NaeTime.Hardware.SQLite;
internal class HardwareDbContext : DbContext
{
    public DbSet<EthernetLapRF8Channel> EthernetLapRF8Channels { get; set; }
    public DbSet<TimerStatus> TimerStatuses { get; set; }
    public DbSet<Detection> Detections { get; set; }
    public DbSet<SerialEsp32Node> SerialEsp32Nodes { get; set; }


    public HardwareDbContext(DbContextOptions<HardwareDbContext> options) : base(options)
    {
    }
}
