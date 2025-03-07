using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.EntityFramework.Models;

namespace NaeTime.Persistence.EntityFramework;
public class NaeTimeDbContext : DbContext
{
    public DbSet<OpenPracticeSession> OpenPracticeSessions { get; set; }
    public DbSet<OpenPracticeLap> OpenPracticeLaps { get; set; }
    public DbSet<OpenPracticeLaneDetection> OpenPracticeDetections { get; set; }
    public DbSet<ConsecutiveLapLeaderboardPosition> ConsecutiveLapLeaderboardPositions { get; set; }
    public DbSet<SingleLapLeaderboardPosition> SingleLapLeaderboardPositions { get; set; }
    public DbSet<TotalLapsLeaderboardPosition> TotalLapsLeaderboardPositions { get; set; }
    public DbSet<AverageLapLeaderboardPosition> AverageLapLeaderboardPositions { get; set; }
    public DbSet<Pilot> Pilots { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<ActiveSession> ActiveSession { get; set; }
    public DbSet<EthernetLapRF8Channel> EthernetLapRF8Channels { get; set; }
    public DbSet<TimerStatus> TimerStatuses { get; set; }
    public DbSet<SerialEsp32Node> SerialEsp32Nodes { get; set; }
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<ActiveTimings> ActiveTimings { get; set; }
    public NaeTimeDbContext(DbContextOptions<NaeTimeDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Track> tracks = modelBuilder.Entity<Track>();
        tracks.OwnsMany(t => t.Timers).WithOwner().HasForeignKey(x => x.TrackId);


        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OpenPracticeSession> sessions = modelBuilder.Entity<OpenPracticeSession>();
        sessions.OwnsMany(s => s.ActiveLanes);
        sessions.OwnsMany(s => s.TrackedConsecutiveLaps);

        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ConsecutiveLapLeaderboardPosition> leaderboards = modelBuilder.Entity<ConsecutiveLapLeaderboardPosition>();
        leaderboards.OwnsMany(x => x.IncludedLaps);

        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ActiveTimings> timings = modelBuilder.Entity<ActiveTimings>();

        timings.OwnsOne(t => t.ActiveLap).WithOwner().HasForeignKey(x => x.ActiveTimingsId);
        timings.OwnsOne(t => t.ActiveSplit).WithOwner().HasForeignKey(x => x.ActiveTimingsId);
    }
}
