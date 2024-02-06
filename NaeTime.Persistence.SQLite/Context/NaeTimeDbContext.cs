using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.SQLite.Models;

namespace NaeTime.Persistence.SQLite.Context;
public class NaeTimeDbContext : DbContext
{
    public DbSet<EthernetLapRF8Channel> EthernetLapRF8Channels { get; set; }
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<Pilot> Pilots { get; set; }
    public DbSet<TimerStatus> TimerStatuses { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Detection> Detections { get; set; }

    public NaeTimeDbContext(DbContextOptions<NaeTimeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var tracks = modelBuilder.Entity<Track>();
        tracks.OwnsMany(t => t.Timers);
    }
}
