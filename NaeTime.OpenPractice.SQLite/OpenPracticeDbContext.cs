namespace NaeTime.OpenPractice.SQLite;

internal class OpenPracticeDbContext : DbContext
{
    public DbSet<OpenPracticeSession> OpenPracticeSessions { get; set; }
    public DbSet<OpenPracticeLap> OpenPracticeLaps { get; set; }
    public DbSet<ConsecutiveLapLeaderboardPosition> ConsecutiveLapLeaderboardPositions { get; set; }
    public OpenPracticeDbContext(DbContextOptions<OpenPracticeDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var sessions = modelBuilder.Entity<OpenPracticeSession>();
        sessions.OwnsMany(s => s.ActiveLanes);
        sessions.OwnsMany(s => s.TrackedConsecutiveLaps);

        var leaderboards = modelBuilder.Entity<ConsecutiveLapLeaderboardPosition>();
        leaderboards.OwnsMany(x => x.IncludedLaps);

    }
}
