namespace NaeTime.OpenPractice.SQLite;

internal class OpenPracticeDbContext : DbContext
{
    public DbSet<OpenPracticeSession> OpenPracticeSessions { get; set; }
    public DbSet<OpenPracticeLap> OpenPracticeLaps { get; set; }
    public DbSet<ConsecutiveLapLeaderboardPosition> ConsecutiveLapLeaderboardPositions { get; set; }
    public DbSet<SingleLapLeaderboardPosition> SingleLapLeaderboardPositions { get; set; }
    public DbSet<TotalLapsLeaderboardPosition> TotalLapsLeaderboardPositions { get; set; }
    public DbSet<AverageLapLeaderboardPosition> AverageLapLeaderboardPositions { get; set; }
    public OpenPracticeDbContext(DbContextOptions<OpenPracticeDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OpenPracticeSession> sessions = modelBuilder.Entity<OpenPracticeSession>();
        sessions.OwnsMany(s => s.ActiveLanes);
        sessions.OwnsMany(s => s.TrackedConsecutiveLaps);

        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ConsecutiveLapLeaderboardPosition> leaderboards = modelBuilder.Entity<ConsecutiveLapLeaderboardPosition>();
        leaderboards.OwnsMany(x => x.IncludedLaps);

    }
}
