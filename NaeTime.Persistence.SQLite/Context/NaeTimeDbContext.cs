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
    public DbSet<ActiveTimings> ActiveTimings { get; set; }
    public DbSet<ActiveSession> ActiveSession { get; set; }

    public NaeTimeDbContext(DbContextOptions<NaeTimeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var tracks = modelBuilder.Entity<Track>();
        tracks.OwnsMany(t => t.Timers).WithOwner().HasForeignKey(x => x.TrackId);

        var timings = modelBuilder.Entity<ActiveTimings>();

        timings.OwnsOne(t => t.ActiveLap).WithOwner().HasForeignKey(x => x.ActiveTimingsId);
        timings.OwnsOne(t => t.ActiveSplit).WithOwner().HasForeignKey(x => x.ActiveTimingsId);
    }
}
