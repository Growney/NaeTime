using Microsoft.EntityFrameworkCore;

namespace NaeTime.Persistence.SQLite.Context;
public class NaeTimeDbContext : DbContext
{

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

        var sessions = modelBuilder.Entity<OpenPracticeSession>();
        sessions.OwnsMany(s => s.ActiveLanes);
        sessions.OwnsMany(s => s.TrackedConsecutiveLaps);

        var lapRecords = modelBuilder.Entity<ConsecutiveLapRecord>();
        lapRecords.OwnsMany(x => x.IncludedLaps);

    }
}
